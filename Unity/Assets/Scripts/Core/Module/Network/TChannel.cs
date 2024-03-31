using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
namespace ET {
    // 封装Socket,将回调push到主线程处理
    public sealed class TChannel: AChannel {
        private readonly TService Service;
        private Socket socket;
        private SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs outArgs = new SocketAsyncEventArgs();
        private readonly CircularBuffer recvBuffer = new CircularBuffer();
        private readonly CircularBuffer sendBuffer = new CircularBuffer();
        private bool isSending;
        private bool isConnected;
        private readonly PacketParser parser;
        private readonly byte[] sendCache = new byte[Packet.OpcodeLength + Packet.ActorIdLength];
        private void OnComplete(object sender, SocketAsyncEventArgs e) {
            this.Service.Queue.Enqueue(new TArgs() {ChannelId = this.Id, SocketAsyncEventArgs = e});
        }
        public TChannel(long id, IPEndPoint ipEndPoint, TService service) {
            this.ChannelType = ChannelType.Connect;
            this.Id = id;
            this.Service = service;
            this.socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.socket.NoDelay = true;
            this.parser = new PacketParser(this.recvBuffer, this.Service);
            this.innArgs.Completed += this.OnComplete;
            this.outArgs.Completed += this.OnComplete;
            this.RemoteAddress = ipEndPoint;
            this.isConnected = false;
            this.isSending = false;
            this.Service.Queue.Enqueue(new TArgs(){Op = TcpOp.Connect,ChannelId = this.Id});
        }
        public TChannel(long id, Socket socket, TService service) {
            this.ChannelType = ChannelType.Accept;
            this.Id = id;
            this.Service = service;
            this.socket = socket;
            this.socket.NoDelay = true;
            this.parser = new PacketParser(this.recvBuffer, this.Service);
            this.innArgs.Completed += this.OnComplete;
            this.outArgs.Completed += this.OnComplete;
            this.RemoteAddress = (IPEndPoint)socket.RemoteEndPoint;
            this.isConnected = true;
            this.isSending = false;
            this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = this.Id});
            this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartRecv, ChannelId = this.Id});
        }
        
        public override void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            Log.Info($"channel dispose: {this.Id} {this.RemoteAddress} {this.Error}");
            long id = this.Id;
            this.Id = 0;
            this.Service.Remove(id);
            this.socket.Close();
            this.innArgs.Dispose();
            this.outArgs.Dispose();
            this.innArgs = null;
            this.outArgs = null;
            this.socket = null;
        }
        public void Send(long actorId, MemoryStream stream) {
            if (this.IsDisposed) {
                throw new Exception("TChannel已经被Dispose, 不能发送消息");
            }
            switch (this.Service.ServiceType) {
				case ServiceType.Inner: { // 内网消息：可以认为，都是跨进程消息，需要信使 actorId
                    int messageSize = (int) (stream.Length - stream.Position);
                    if (messageSize > ushort.MaxValue * 16) {
                        throw new Exception($"send packet too large: {stream.Length} {stream.Position}");
                    }
                    this.sendCache.WriteTo(0, messageSize);
                    this.sendBuffer.Write(this.sendCache, 0, PacketParser.InnerPacketSizeLength);
                    // actorId
                    stream.GetBuffer().WriteTo(0, actorId);
                    this.sendBuffer.Write(stream.GetBuffer(), (int)stream.Position, (int)(stream.Length - stream.Position));
                    break;
                }
				case ServiceType.Outer: { // 外网消息
                    stream.Seek(Packet.ActorIdLength, SeekOrigin.Begin); // 外网不需要actorId
                    ushort messageSize = (ushort) (stream.Length - stream.Position);
                    this.sendCache.WriteTo(0, messageSize);
                    this.sendBuffer.Write(this.sendCache, 0, PacketParser.OuterPacketSizeLength);
                    
                    this.sendBuffer.Write(stream.GetBuffer(), (int)stream.Position, (int)(stream.Length - stream.Position));
                    break;
                }
            }
            if (!this.isSending) { // 这里想明白：什么情况下，会是正在发送？开始发送后，一次发不完，不得不多个包裹发送同一个消息？【TODO】：确认一下
                // this.StartSend();
                this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = this.Id});
            }
        }
        public void ConnectAsync() {
            this.outArgs.RemoteEndPoint = this.RemoteAddress;
            if (this.socket.ConnectAsync(this.outArgs)) {
                return;
            }
            OnConnectComplete(this.outArgs);
        }
        public void OnConnectComplete(SocketAsyncEventArgs e) {
            if (this.socket == null) {
                return;
            }
            if (e.SocketError != SocketError.Success) {
                this.OnError((int)e.SocketError);    
                return;
            }
            e.RemoteEndPoint = null;
            this.isConnected = true;
            this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = this.Id});
            this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartRecv, ChannelId = this.Id});
        }
        public void OnDisconnectComplete(SocketAsyncEventArgs e) {
            this.OnError((int)e.SocketError);
        }
        public void StartRecv() {
            while (true) {
                try {
                    if (this.socket == null)
                    {
                        return;
                    }
                    
                    int size = this.recvBuffer.ChunkSize - this.recvBuffer.LastIndex;
                    this.innArgs.SetBuffer(this.recvBuffer.Last, this.recvBuffer.LastIndex, size);
                }
                catch (Exception e) {
                    Log.Error($"tchannel error: {this.Id}\n{e}");
                    this.OnError(ErrorCore.ERR_TChannelRecvError);
                    return;
                }
                if (this.socket.ReceiveAsync(this.innArgs)) {
                    return;
                }
                this.HandleRecv(this.innArgs);
            }
        }
        public void OnRecvComplete(SocketAsyncEventArgs o) {
            this.HandleRecv(o);
            if (this.socket == null) {
                return;
            }
            this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartRecv, ChannelId = this.Id});
        }
        private void HandleRecv(SocketAsyncEventArgs e) {
            if (this.socket == null) {
                return;
            }
            if (e.SocketError != SocketError.Success) {
                this.OnError((int)e.SocketError);
                return;
            }
            if (e.BytesTransferred == 0) {
                this.OnError(ErrorCore.ERR_PeerDisconnect);
                return;
            }
            this.recvBuffer.LastIndex += e.BytesTransferred;
            if (this.recvBuffer.LastIndex == this.recvBuffer.ChunkSize) {
                this.recvBuffer.AddLast();
                this.recvBuffer.LastIndex = 0;
            }
            // 收到消息回调
            while (true) {
                // 这里循环解析消息执行，有可能，执行消息的过程中断开了session
                if (this.socket == null) {
                    return;
                }
                try {
                    bool ret = this.parser.Parse();
                    if (!ret)
                    {
                        break;
                    }
                    
                    this.OnRead(this.parser.MemoryStream);
                }
                catch (Exception ee) {
                    Log.Error($"ip: {this.RemoteAddress} {ee}");
                    this.OnError(ErrorCore.ERR_SocketError);
                    return;
                }
            }
        }
        public void StartSend() {
            if (!this.isConnected) {
                return;
            }
            if (this.isSending) { // 正在发送，就不用继续了
                return;
            }
            while (true) {
                try {
                    if (this.socket == null) {
                        this.isSending = false;
                        return;
                    }
                    // 没有数据需要发送
                    if (this.sendBuffer.Length == 0) {
                        this.isSending = false;
                        return;
                    }
                    this.isSending = true; // 标记
                    int sendSize = this.sendBuffer.ChunkSize - this.sendBuffer.FirstIndex;
                    if (sendSize > this.sendBuffer.Length) {
                        sendSize = (int)this.sendBuffer.Length;
                    }
                    this.outArgs.SetBuffer(this.sendBuffer.First, this.sendBuffer.FirstIndex, sendSize);
					// 没看明白：下面的2 种情况，2 个分支。
					// 第一个判断【SocketError == SocketError.IOPending】是IO 出错异常了？如果SocketIO 异常了，这个消息会怎么样？
                    if (this.socket.SendAsync(this.outArgs)) { // 去看SendAsync() 细节：更底层的【管道 Socket】上的发送，IO 操作之类的，不细看了
                        return;
                    }
                    HandleSend(this.outArgs); // 【TODO】：这里暂且假定，发送缓存区、多个块的内容，是根据消息的长度，自动连续发送的
                }
                catch (Exception e) {
                    throw new Exception($"socket set buffer error: {this.sendBuffer.First.Length}, {this.sendBuffer.FirstIndex}", e);
                }
            }
        }
// 每发送一小段【不及1 整块、或是1 整块】，都会触发这个【每发送1 小段结束】的回调，一个大消息可以回调N 次
        public void OnSendComplete(SocketAsyncEventArgs o) { 
            HandleSend(o); // 快进【发送成功了的、这段消息的长度】
            this.isSending = false; // 标记：现在没在发
            this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = this.Id}); // 缓存：请求发送1 条大消息的、下一段。。那么1 条大消息分N 桢发送完。。 
        }
        private void HandleSend(SocketAsyncEventArgs e) {
            if (this.socket == null) { // 管道空
                return;
            }
            if (e.SocketError != SocketError.Success) { // 管道出错了
                this.OnError((int)e.SocketError);
                return;
            }
            if (e.BytesTransferred == 0) { // 数据没能发送成功：管道另一端、一定是断联了！
                this.OnError(ErrorCore.ERR_PeerDisconnect);
                return;
            }
            this.sendBuffer.FirstIndex += e.BytesTransferred; // 发送了一段
            if (this.sendBuffer.FirstIndex == this.sendBuffer.ChunkSize) { // 一个节点块的内容，发光了，去头，发下一个节点块的内容；可能完全发送完了【触发发送结束回调】！
                this.sendBuffer.FirstIndex = 0;
                this.sendBuffer.RemoveFirst();
            } // 没懂的是：一个节点块的内容，发完了；后续节点块，是根据消息的长度，自动连续发送、自动完成的？【TODO】：去找
        }
        private void OnRead(MemoryStream memoryStream) { // 信道上：感觉这个【自底向上】自管道向上、向信道、向服务、向主线程、同步的过程
            try {
                long channelId = this.Id;
                object message = null;
                long actorId = 0;
                switch (this.Service.ServiceType) {
					case ServiceType.Outer: { // 外网消息
                        ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.KcpOpcodeIndex);
                        Type type = NetServices.Instance.GetType(opcode); // 【操作码】 ==> 【外网消息】的、消息类型
                        message = SerializeHelper.Deserialize(type, memoryStream); // 把当前讲到的内存流，反序列化成、特定的消息类型
                        break;
                    }
					case ServiceType.Inner: { // 内网消息：理解为，跨进程消息，带 actorId 
                        actorId = BitConverter.ToInt64(memoryStream.GetBuffer(), Packet.ActorIdIndex);
                        ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.OpcodeIndex);
                        Type type = NetServices.Instance.GetType(opcode);
                        message = SerializeHelper.Deserialize(type, memoryStream);
                        break;
                    }
                }
                NetServices.Instance.OnRead(this.Service.Id, channelId, actorId, message); // 去看这个亲爱的表哥的活宝妹，先前迷迷糊糊的过程。。【TODO】：现在
            }
            catch (Exception e) {
                Log.Error($"{this.RemoteAddress} {memoryStream.Length} {e}");
                // 出现任何消息解析异常都要断开Session，防止客户端伪造消息
                this.OnError(ErrorCore.ERR_PacketParserError);
            }
        }
        private void OnError(int error) {
            Log.Info($"TChannel OnError: {error} {this.RemoteAddress}");
            long channelId = this.Id;
            this.Service.Remove(channelId);
            NetServices.Instance.OnError(this.Service.Id, channelId, error);
        }
    }
}