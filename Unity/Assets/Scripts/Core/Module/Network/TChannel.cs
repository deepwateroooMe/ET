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

        // 读写缓存区：是它们操作的缓存区，不是内存流，内存流是框架提升性能采用的帮助工具
        private readonly CircularBuffer recvBuffer = new CircularBuffer();
        private readonly CircularBuffer sendBuffer = new CircularBuffer();

        private bool isSending;
        private bool isConnected;

        private readonly PacketParser parser;
        private readonly byte[] sendCache = new byte[Packet.OpcodeLength + Packet.ActorIdLength];

        private void OnComplete(object sender, SocketAsyncEventArgs e) {
            this.Service.Queue.Enqueue(new TArgs() {ChannelId = this.Id, SocketAsyncEventArgs = e}); // 去验证：它也是会周期性执行回调函数的
        }
        
        public TChannel(long id, IPEndPoint ipEndPoint, TService service) {
            this.ChannelType = ChannelType.Connect; // 连接通信
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
            this.ChannelType = ChannelType.Accept; // 接受通道
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
        public void Send(long actorId, MemoryStream stream) { // 框架把数据封装在内存流，发送是从内存流中读出，写入发送缓存区，由发送缓存区中发送出去
            if (this.IsDisposed) {
                throw new Exception("TChannel已经被Dispose, 不能发送消息");
            }
            switch (this.Service.ServiceType) {
                case ServiceType.Inner: {
                    int messageSize = (int) (stream.Length - stream.Position);
                    if (messageSize > ushort.MaxValue * 16)
                    {
                        throw new Exception($"send packet too large: {stream.Length} {stream.Position}");
                    }
                    this.sendCache.WriteTo(0, messageSize); // 把大小写在0位
                    this.sendBuffer.Write(this.sendCache, 0, PacketParser.InnerPacketSizeLength); // 把数据准备缓存区sendCache 中的内容写入真正的发送缓存区sendBuffer 
                    // actorId
                    stream.GetBuffer().WriteTo(0, actorId); // 更新内存流中的 actorId
                    this.sendBuffer.Write(stream.GetBuffer(), (int)stream.Position, (int)(stream.Length - stream.Position)); // 将内存流中的缓存区数据 写入当前真正的发送缓存区
                    break;
                }
                case ServiceType.Outer: {
                    stream.Seek(Packet.ActorIdLength, SeekOrigin.Begin); // 外网不需要actorId，内存流指针就向前前移这个长度，8个字节
                    ushort messageSize = (ushort) (stream.Length - stream.Position); // 消息长度
                    this.sendCache.WriteTo(0, messageSize); // 先写长度
                    this.sendBuffer.Write(this.sendCache, 0, PacketParser.OuterPacketSizeLength); // 再写外网消息的长度2 

                    this.sendBuffer.Write(stream.GetBuffer(), (int)stream.Position, (int)(stream.Length - stream.Position)); // 接着写，消息体
                    break;
                }
            }
            if (!this.isSending) {
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
            // 当连接建立好了，就可以发送接收消息了呀            
            this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = this.Id});
            this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartRecv, ChannelId = this.Id});
        }
        public void OnDisconnectComplete(SocketAsyncEventArgs e) {
            this.OnError((int)e.SocketError);
        }
        public void StartRecv() {
            while (true) {
                try {
                    if (this.socket == null) {
                        return;
                    }
                    // 按大块发送的理论：块的下标索引，当前块内的字节下标索引，设置大小                    
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
                this.OnError((int)e.SocketError); // 抛错
                return;
            }
            if (e.BytesTransferred == 0) {
                this.OnError(ErrorCore.ERR_PeerDisconnect);
                return;
            }
            this.recvBuffer.LastIndex += e.BytesTransferred;
            // 这里，发送的大块刚好发送到一个大块完，接收的块应该同样也是到一个大块完，应该不会存在发送至发送缓存一个大块儿完，但是接到缓存写当前大块写不完的情况，两者应该是同左右移同进退的
            if (this.recvBuffer.LastIndex == this.recvBuffer.ChunkSize) {
                this.recvBuffer.AddLast();
                this.recvBuffer.LastIndex = 0; // 在一个新的块里，块内字节下标设置为0
            }
            // 收到消息回调
            while (true) {
                // 这里循环解析消息执行，有可能，执行消息的过程中断开了session
                if (this.socket == null) {
                    return;
                }
                try {
                    bool ret = this.parser.Parse();
                    if (!ret) {
                        break;
                    }
                    this.OnRead(this.parser.MemoryStream); // 框架的封装，内存流流式读写
                }
                catch (Exception ee) {
                    Log.Error($"ip: {this.RemoteAddress} {ee}");
                    this.OnError(ErrorCore.ERR_SocketError);
                    return;
                }
            }
        }
        public void StartSend() {
            if(!this.isConnected) {
                return;
            }
            if (this.isSending) {
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
                    // 框架采用的数据结构圆形缓存区，块式发送
                    this.isSending = true;
                    int sendSize = this.sendBuffer.ChunkSize - this.sendBuffer.FirstIndex;
                    if (sendSize > this.sendBuffer.Length) { // TODO: 这里始终没有想明白，什么情况下，才会出现这样的情况？
                        sendSize = (int)this.sendBuffer.Length;
                    }
                    this.outArgs.SetBuffer(this.sendBuffer.First, this.sendBuffer.FirstIndex, sendSize);
                    if (this.socket.SendAsync(this.outArgs)) {
                        return;
                    }
                    HandleSend(this.outArgs);
                }
                catch (Exception e) {
                    throw new Exception($"socket set buffer error: {this.sendBuffer.First.Length}, {this.sendBuffer.FirstIndex}", e);
                }
            }
        }
        public void OnSendComplete(SocketAsyncEventArgs o) {
            HandleSend(o);
            this.isSending = false;
            this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = this.Id});
        }
        private void HandleSend(SocketAsyncEventArgs e) {
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
            this.sendBuffer.FirstIndex += e.BytesTransferred;
            if (this.sendBuffer.FirstIndex == this.sendBuffer.ChunkSize) {
                this.sendBuffer.FirstIndex = 0;
                this.sendBuffer.RemoveFirst();
            }
        }
        private void OnRead(MemoryStream memoryStream) {
            try {
                long channelId = this.Id;
                object message = null;
                long actorId = 0;
                switch (this.Service.ServiceType) {
                    case ServiceType.Outer: {
                        ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.KcpOpcodeIndex);
                        Type type = NetServices.Instance.GetType(opcode);
                        message = SerializeHelper.Deserialize(type, memoryStream);
                        break;
                    }
                    case ServiceType.Inner: {
                        actorId = BitConverter.ToInt64(memoryStream.GetBuffer(), Packet.ActorIdIndex);
                        ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.OpcodeIndex);
                        Type type = NetServices.Instance.GetType(opcode);
                        message = SerializeHelper.Deserialize(type, memoryStream); // 从内存流中反序列出消息
                        break;
                    }
                }
                NetServices.Instance.OnRead(this.Service.Id, channelId, actorId, message); // 传输交待主线程，我网络线程读到数据了 ?
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