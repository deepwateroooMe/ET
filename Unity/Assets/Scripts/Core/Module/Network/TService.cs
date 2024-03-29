using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
namespace ET {
    public enum TcpOp {
        StartSend,
        StartRecv,
        Connect,
    }
    public struct TArgs {
        public TcpOp Op;
        public long ChannelId;
        public SocketAsyncEventArgs SocketAsyncEventArgs;
    }
    public sealed class TService : AService { // 一种，实体类：有此类型【双端建立连接的几次握手什么的。。】
        private readonly Dictionary<long, TChannel> idChannels = new Dictionary<long, TChannel>();
        private readonly SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
        private Socket acceptor; // TService 服务端，专门用来接收，这种服务TService 的客户端的 Socket ？
        public ConcurrentQueue<TArgs> Queue = new ConcurrentQueue<TArgs>(); // 用来管理，这种TService 服务端可能有的、最多1000 个客户端的各种使用用例请求
        public TService(AddressFamily addressFamily, ServiceType serviceType) {
            this.ServiceType = serviceType;
        }
        public TService(IPEndPoint ipEndPoint, ServiceType serviceType) {
            this.ServiceType = serviceType;
            this.acceptor = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // 开个，Tcp流式管道
            // 容易出问题，先注释掉，按需开启【优化的话：大概，双端重连的情况下，能够省掉一些不必要的麻烦步骤过程】
            // this.acceptor.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.innArgs.Completed += this.OnComplete;
            try {
                this.acceptor.Bind(ipEndPoint); // 服务端地址
            } catch (Exception e) {
                throw new Exception($"bind error: {ipEndPoint}", e);
            }
            this.acceptor.Listen(1000); // 可同步最多连 1000 个客户端
            this.AcceptAsync(); // 开始：异步监听、并接受客户端
        }
        private void OnComplete(object sender, SocketAsyncEventArgs e) {
            switch (e.LastOperation) {
            case SocketAsyncOperation.Accept:
                this.Queue.Enqueue(new TArgs() {SocketAsyncEventArgs = e}); // <<<<<<<<<<<<<<<<<<<< 
                break;
            default:
                throw new Exception($"socket error: {e.LastOperation}");
            }
        }
        private void OnAcceptComplete(SocketError socketError, Socket acceptSocket) {
            if (this.acceptor == null) 
                return;
            if (socketError != SocketError.Success) {
                Log.Error($"accept error {socketError}");
                return;
            }
            try { // 网络通信【创建信道】：网络通信底层，建立通信之前，创建一个通信信道的过程
                long id = NetServices.Instance.CreateAcceptChannelId(); // 这个，当作了 serviceId 在使用
                TChannel channel = new TChannel(id, acceptSocket, this);
                this.idChannels.Add(channel.Id, channel);
                long channelId = channel.Id; // 服务号、信道号、客户端地址
				// 下面：NetServices 实例，可是是服务端的，也可以是客户端的，各用自己的逻辑。它是个【服务端】但它同时也是个【异步线程】除非通知主线程，主线程不知道它在干什么。。
                NetServices.Instance.OnAccept(this.Id, channelId, channel.RemoteAddress); // 创建后，加入总管主线程的、网络模块的、管理里。去总管里看下
            }
            catch (Exception exception) {
                Log.Error(exception);
            }        
            // 开始新的accept
            this.AcceptAsync();
        }
        private void AcceptAsync() { // 回想：双端连接时，异步接收的过程细节。。
            this.innArgs.AcceptSocket = null;
            if (this.acceptor.AcceptAsync(this.innArgs)) // 太偏底层了。。 
                return;
            OnAcceptComplete(this.innArgs.SocketError, this.innArgs.AcceptSocket);
        }
        private TChannel Create(IPEndPoint ipEndPoint, long id) {
            TChannel channel = new TChannel(id, ipEndPoint, this);
            this.idChannels.Add(channel.Id, channel);
            return channel;
        }
         public override void Create(long id, IPEndPoint address) {
            if (this.idChannels.TryGetValue(id, out TChannel _)) 
                return;
            this.Create(address, id);
        }
        private TChannel Get(long id) {
            TChannel channel = null;
            this.idChannels.TryGetValue(id, out channel);
            return channel;
        }
        public override void Dispose() {
            base.Dispose();
            this.acceptor?.Close();
            this.acceptor = null;
            this.innArgs.Dispose();
            foreach (long id in this.idChannels.Keys.ToArray()) {
                TChannel channel = this.idChannels[id];
                channel.Dispose();
            }
            this.idChannels.Clear();
        }
        public override void Remove(long id, int error = 0) {
            if (this.idChannels.TryGetValue(id, out TChannel channel)) {
                channel.Error = error;
                channel.Dispose();    
            }
            this.idChannels.Remove(id);
        }
		// 重点方法：
        public override void Send(long channelId, long actorId, object message) {
            try {
                TChannel aChannel = this.Get(channelId);
                if (aChannel == null) { // 信道不存在：报错
                    NetServices.Instance.OnError(this.Id, channelId, ErrorCore.ERR_SendMessageNotFoundTChannel);
                    return;
                }
                MemoryStream memoryStream = this.GetMemoryStream(message); // 序列化消息到内存流，并缓存最后一消息 
                // 【信道上发送消息】：涉及，进程间消息的（凡用信道发消息，一定是跨进程的吗？），
                // 发送前消息的【序列化】与，接收消息后的【反序列化】。
                // 先前源者标了个【内存流上发消息，缓存最后一条发送过的消息】【客户端】利与不利？源者说，客户端的发送消息，是狠少的。。。
                aChannel.Send(actorId, memoryStream); // 【信道】上发送消息，发到信道的另一头远程【客户端】？去看一下
            }
            catch (Exception e) {
                Log.Error(e);
            }
        }
        public override void Update() { // 每桢执行：纵多1000 个客户端的、各种用例请求
            while (true) {
                if (!this.Queue.TryDequeue(out var result)) // 队列里，抓下一个待执行请求出来处理
                    break;
                SocketAsyncEventArgs e = result.SocketAsyncEventArgs;
                if (e == null) {
                    switch (result.Op) {
                    case TcpOp.StartSend: { // 【开始发送】请求
                        TChannel tChannel = this.Get(result.ChannelId); // 客户端对应的、特异信道
                        if (tChannel != null) {
                            tChannel.StartSend(); // 信道上发消息，非常底层不用看
                        }
                        break;
                    }
                    case TcpOp.StartRecv: {
                        TChannel tChannel = this.Get(result.ChannelId);
                        if (tChannel != null) {
                            tChannel.StartRecv();
                        }
                        break;
                    }
                    case TcpOp.Connect: {
                        TChannel tChannel = this.Get(result.ChannelId);
                        if (tChannel != null) {
                            tChannel.ConnectAsync();
                        }
                        break;
                    }
                    }
                    continue;
                }
                switch (e.LastOperation) {
                case SocketAsyncOperation.Accept: {
                    SocketError socketError = e.SocketError;
                    Socket acceptSocket = e.AcceptSocket;
                    this.OnAcceptComplete(socketError, acceptSocket);
                    break;
                }
                case SocketAsyncOperation.Connect: {
                    TChannel tChannel = this.Get(result.ChannelId);
                    if (tChannel != null) {
                        tChannel.OnConnectComplete(e);
                    }
                    break;
                }
                case SocketAsyncOperation.Disconnect: {
                    TChannel tChannel = this.Get(result.ChannelId);
                    if (tChannel != null) {
                        tChannel.OnDisconnectComplete(e);
                    }
                    break;
                }
                case SocketAsyncOperation.Receive: {
                    TChannel tChannel = this.Get(result.ChannelId);
                    if (tChannel != null) {
                        tChannel.OnRecvComplete(e);
                    }
                    break;
                }
                case SocketAsyncOperation.Send: {
                    TChannel tChannel = this.Get(result.ChannelId);
                    if (tChannel != null) {
                        tChannel.OnSendComplete(e);
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException($"{e.LastOperation}");
                }
            }
        }
        public override bool IsDispose() {
            return this.acceptor == null;
        }
    }
}