﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
namespace ET {

    // 封装Socket,将回调push到主线程处理
    public sealed class TChannel: AChannel { // 信道上的两端、每一端：都可能有进进出出来来往往的消息，两个方向都管理
        private readonly TService Service;
        private Socket socket;
        private SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs outArgs = new SocketAsyncEventArgs();
        private readonly CircularBuffer recvBuffer = new CircularBuffer(); // 消息缓存区
        private readonly CircularBuffer sendBuffer = new CircularBuffer();
        private bool isSending;
        private bool isConnected;
        private readonly PacketParser parser;
        private readonly byte[] sendCache = new byte[Packet.OpcodeLength + Packet.ActorIdLength]; // 发送缓存区
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
            this.Service.Queue.Enqueue(new TArgs(){Op = TcpOp.Connect, ChannelId = this.Id});
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
        public void Send(long actorId, MemoryStream stream) { // 【发送过程】：把内存流上的消息，从【内存缓存区】转移到【发送缓存区】的过程。
            if (this.IsDisposed) {
                throw new Exception("TChannel已经被Dispose, 不能发送消息");
            }
            switch (this.Service.ServiceType) { // 区分：【内网消息】与【外网消息】。将序列化后的数据，存放至【发送缓存区】待发
                case ServiceType.Inner: { // 信道【服务端】的内网消息：是发向其它服务端的
                    int messageSize = (int) (stream.Length - stream.Position);
                    if (messageSize > ushort.MaxValue * 16)
                        throw new Exception($"send packet too large: {stream.Length} {stream.Position}");
                    this.sendCache.WriteTo(0, messageSize);
                    this.sendBuffer.Write(this.sendCache, 0, PacketParser.InnerPacketSizeLength);
                    // actorId: 写进内存流里
                    stream.GetBuffer().WriteTo(0, actorId); // Inner 类型：在开头的 0 位置，写 actorId. 这个号，【发送消息】与【返回消息】一一对应不变
                    this.sendBuffer.Write(stream.GetBuffer(), (int)stream.Position, (int)(stream.Length - stream.Position));
                    break;
                }
				case ServiceType.Outer: { // 【外网消息】：是服务端发向客户端的
                    stream.Seek(Packet.ActorIdLength, SeekOrigin.Begin); // 外网不需要actorId: 【不需要信使，双端间有信道了直接发】 
                    ushort messageSize = (ushort) (stream.Length - stream.Position);
                    this.sendCache.WriteTo(0, messageSize);
                    this.sendBuffer.Write(this.sendCache, 0, PacketParser.OuterPacketSizeLength);
                    this.sendBuffer.Write(stream.GetBuffer(), (int)stream.Position, (int)(stream.Length - stream.Position));
                    break;
                }
            }
            if (!this.isSending) {
                // this.StartSend();
                this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = this.Id}); // 缓存了：开始发送的请求。每桢Update() 时会执行
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
            if (!this.isConnected) // 信道若是断开的，无法执行
                return;
            if (this.isSending) // 怎么会有这种逻辑的？一个包裹可以狠大，信道发送区一次？尚且发不完，得发好多次。再发第2 次【发送区大小时】就不再执行这里了？
                return;
            while (true) {
                try {
                    if (this.socket == null) {
                        this.isSending = false;
                        return;
                    }
                    // 没有数据需要发送
                    if (this.sendBuffer.Length == 0) { // 一个发送请求的包裹，终于发送完了
                        this.isSending = false;
                        return;
                    }
                    this.isSending = true; // 开始发包裹
                    int sendSize = this.sendBuffer.ChunkSize - this.sendBuffer.FirstIndex;
                    if (sendSize > this.sendBuffer.Length)
                        sendSize = (int)this.sendBuffer.Length;
                    this.outArgs.SetBuffer(this.sendBuffer.First, this.sendBuffer.FirstIndex, sendSize);
                    if (this.socket.SendAsync(this.outArgs))
                        return;
                    HandleSend(this.outArgs); // 逻辑在下面的函数里
                }
                catch (Exception e) {
                    throw new Exception($"socket set buffer error: {this.sendBuffer.First.Length}, {this.sendBuffer.FirstIndex}", e);
                }
            }
        }
        public void OnSendComplete(SocketAsyncEventArgs o) { // 哪里会调用这里呢？
            HandleSend(o);
            this.isSending = false;
            this.Service.Queue.Enqueue(new TArgs() { Op = TcpOp.StartSend, ChannelId = this.Id});
        }
        private void HandleSend(SocketAsyncEventArgs e) {
            if (this.socket == null) 
                return;
            if (e.SocketError != SocketError.Success) {
                this.OnError((int)e.SocketError);
                return;
            }
            if (e.BytesTransferred == 0) {
                this.OnError(ErrorCore.ERR_PeerDisconnect);
                return;
            }
            this.sendBuffer.FirstIndex += e.BytesTransferred; // 标记：接下来还需要发送的、剩余包裹的起始下标位置
            if (this.sendBuffer.FirstIndex == this.sendBuffer.ChunkSize) { // 一块一块地发：当前块的内容全发了，就如链条、移除头链条节点，接下来会、接着发链条下一节点块的内容
                this.sendBuffer.FirstIndex = 0;
                this.sendBuffer.RemoveFirst();
            }
        }
        private void OnRead(MemoryStream memoryStream) {
            try {
                long channelId = this.Id;
                object message = null;
                long actorId = 0;
                switch (this.Service.ServiceType) { // 这里 Outer: 更像是本进程内的消息？发送前，与收到后的 actorId ＝ 0 手动写的 
                    case ServiceType.Outer: { // actorId ＝ 0: 前面【会话框Session.cs】上发送消息的时候，也曾自动写 actorId ＝ 0 过。没能理解这个细节是什么意思 0 ？
                            ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.KcpOpcodeIndex);
                            Type type = NetServices.Instance.GetType(opcode);
                            message = SerializeHelper.Deserialize(type, memoryStream);
                            break;
                        }
                    case ServiceType.Inner: { // 【内网消息】：记不用服务器间的、信使特异号
                        actorId = BitConverter.ToInt64(memoryStream.GetBuffer(), Packet.ActorIdIndex); // 这个应该就是从 0 位开始读号  // <<<<<<<<<<<<<<<<<<<< 
                            ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.OpcodeIndex);
                            Type type = NetServices.Instance.GetType(opcode);
                            message = SerializeHelper.Deserialize(type, memoryStream);
                            break;
                        }
                }
                // 把这里【收到消息】时，actorId 重新生成的过程，弄明白。可是这个方法跟进去，并不曾真正使用 actorId 变量！！现在还在哪里迷糊呢？
				// 收到读到消息【自底层向上】：自异步线程、某端的底层，向上同步到主线程的过程
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
            this.Service.Remove(channelId); // 出错的信道：废弃不用、移除掉
            NetServices.Instance.OnError(this.Service.Id, channelId, error); // 回调通知主线程，可能回调到客户端？信道出错了，怎么可能回调到客户端的？不可能
        }
    }
}