using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
namespace ET {
    // 封装Socket,将回调push到主线程处理【源作者标注】. 主要用于绑定一个socket连接，并管理这个socket消息的发送与接收
    public sealed class TChannel: AChannel {
        private readonly TService Service;
        private Socket socket;

// “SocketAsyncEventArgs”的用法与Tservice一样，CircularBuffer类的主要思想就是循环利用字节数组，用于处理steam流，从而做到0GC        
        // 其内拥有两个SocketAsyncEventArgs类实例innArgs,outArgs,
        private SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs outArgs = new SocketAsyncEventArgs();
        // 对应的拥有两个ET6.0封装好的CircularBuffer类实例recvBuffer与sendBuffer，用于处理来自socket发来的steam流转换为字节流的处理
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
// 从Session 开始的过程：
    // 调用发送方法，根据内外网做区分。
    // 使用MessageSerializeHelper类，序列化对象到stream流，同时也包含反序列化steam流到对象的功能，还封装了协议号进stream中。
    // 接着调用对应的Service发送，转接到对应的TChannel，进行底层socket发送，需要注意在TChannel发送时，会主动加入协议大小进stream流中
        public void Send(long actorId, MemoryStream stream) { // 从信道上发消息：
            if (this.IsDisposed) {
                throw new Exception("TChannel已经被Dispose, 不能发送消息");
            }
            switch (this.Service.ServiceType) {
                case ServiceType.Inner: {
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
                case ServiceType.Outer: {
                    stream.Seek(Packet.ActorIdLength, SeekOrigin.Begin); // 外网不需要actorId
                    ushort messageSize = (ushort) (stream.Length - stream.Position);
                    this.sendCache.WriteTo(0, messageSize);
                    this.sendBuffer.Write(this.sendCache, 0, PacketParser.OuterPacketSizeLength);
                    this.sendBuffer.Write(stream.GetBuffer(), (int)stream.Position, (int)(stream.Length - stream.Position));
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
                    bool ret = this.parser.Parse(); // 这个是分阶段解析，先解析出个头，大概是方便内网消息的直接内存流上转发，不用再反序列人，又重复序列化
                    if (!ret) {
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
        // 其中有很多有关字节的细节处理，相关类：这些别人强调过的细节，我觉得自己都读懂了
        // PacketParser从CircularBuffer解析数据包数据（协议数据开头都包含了消息数据大小），
        // CircularBuffer用于循环接受socket消息，能够分段（多个字节数组）存储数据，读取又能从这些段中取出之前存储的数据
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
                    this.isSending = true;
                    int sendSize = this.sendBuffer.ChunkSize - this.sendBuffer.FirstIndex;
                    if (sendSize > this.sendBuffer.Length) {
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
                    message = SerializeHelper.Deserialize(type, memoryStream);
                    break;
                }
                }
                NetServices.Instance.OnRead(this.Service.Id, channelId, actorId, message);
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