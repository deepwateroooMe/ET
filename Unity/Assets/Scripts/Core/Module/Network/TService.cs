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
// 与下面的某个前 NetKcpComponentSystem 有类似的地方，可以拿来参考总结一下： 
    // 初始化消息派发器为外网派发器：OuterMessageDispatcher
    // 创建一个网络服务器TService
    // 初始化网络服务器的几个Action，用于将网络服务器消息反应到NetKcpComponent扩展方法。
    // 将创建的网络服务器加到NetThreadComponent组件中，用于NetThreadComponent来驱动网络服务器。
    // 其中需要关注的方法为：OnRead与OnAccept方法。
    // OnAccept方法用于创建好一条新的对外连接Socket时，执行此方法，创建一个与之关联的Session实例。
    // OnRead方法用于当收到来自某个外网连接的数据时，接受到MemoryStream流数据，找到关联的Session类与对应的流数据，通过消息派发器进行数据派发。
    public sealed class TService : AService {
        private readonly Dictionary<long, TChannel> idChannels = new Dictionary<long, TChannel>();
        private readonly SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
        private Socket acceptor;
        public ConcurrentQueue<TArgs> Queue = new ConcurrentQueue<TArgs>();

        public TService(AddressFamily addressFamily, ServiceType serviceType) {
            this.ServiceType = serviceType;
        }
        public TService(IPEndPoint ipEndPoint, ServiceType serviceType) { // 这个方法的解释，更多的是源自先前旧版本，参考一下就可以了
            this.ServiceType = serviceType; // 类型：内网消息，外网消息 ?
            // 新建一个socket，并开启监听方式，来监听来自他socket的连接，用于提供网络服务功能
            this.acceptor = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // 容易出问题，先注释掉，按需开启
            // this.acceptor.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.innArgs.Completed += this.OnComplete; // 设置SocketAsyncEventArgs类实例innArgs的异步完成回调

            // 如果检测到有其他连接，分支成是否异步完成（异步完成，交由innArgs实例的OnComplete方法处理，扔到主线程执行OnAcceptComplete方法；非异步完成，直接调用OnAcceptComplete方法，开启下一轮连接检测。
            // 早版本有个逻辑是：将AcceptAsync方法扔到主线程启动开始接受监听。当前版本是如何完成的？这个版本分成了主线程，与网络异步线程，并将网络异步线程的结果同步到主线程的做法
            // this.ThreadSynchronizationContext.PostNext(this.AcceptAsync);  // 这里早版本里的实现

            try {
                this.acceptor.Bind(ipEndPoint);
            }
            catch (Exception e) {
                throw new Exception($"bind error: {ipEndPoint}", e);
            }
            this.acceptor.Listen(1000);
            this.AcceptAsync();
        }
        private void OnComplete(object sender, SocketAsyncEventArgs e) {
            switch (e.LastOperation) {
            case SocketAsyncOperation.Accept:
                this.Queue.Enqueue(new TArgs() {SocketAsyncEventArgs = e});
                break;
            default:
                throw new Exception($"socket error: {e.LastOperation}");
            }
        }
        private void OnAcceptComplete(SocketError socketError, Socket acceptSocket) {
            if (this.acceptor == null) {
                return;
            }
            if (socketError != SocketError.Success) {
                Log.Error($"accept error {socketError}");
                return;
            }
            try {
                long id = NetServices.Instance.CreateAcceptChannelId();
                TChannel channel = new TChannel(id, acceptSocket, this);
                this.idChannels.Add(channel.Id, channel);
                long channelId = channel.Id;
                
                NetServices.Instance.OnAccept(this.Id, channelId, channel.RemoteAddress);
            }
            catch (Exception exception) {
                Log.Error(exception);
            }        
            
            // 开始新的accept
            this.AcceptAsync();
        }
        
        private void AcceptAsync() {
            this.innArgs.AcceptSocket = null;
            if (this.acceptor.AcceptAsync(this.innArgs)) {
                return;
            }
            OnAcceptComplete(this.innArgs.SocketError, this.innArgs.AcceptSocket);
        }
        private TChannel Create(IPEndPoint ipEndPoint, long id) {
            TChannel channel = new TChannel(id, ipEndPoint, this);
            this.idChannels.Add(channel.Id, channel);
            return channel;
        }
        public override void Create(long id, IPEndPoint address) {
            if (this.idChannels.TryGetValue(id, out TChannel _)) {
                return;
            }
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
        public override void Send(long channelId, long actorId, object message) {
            try {
                TChannel aChannel = this.Get(channelId);
                if (aChannel == null) {
                    NetServices.Instance.OnError(this.Id, channelId, ErrorCore.ERR_SendMessageNotFoundTChannel);
                    return;
                }
                MemoryStream memoryStream = this.GetMemoryStream(message);
                aChannel.Send(actorId, memoryStream);
            }
            catch (Exception e) {
                Log.Error(e);
            }
        }
        public override void Update() {
            while (true) {
                if (!this.Queue.TryDequeue(out var result)) {
                    break;
                }
                SocketAsyncEventArgs e = result.SocketAsyncEventArgs;
                if (e == null) {
                    switch (result.Op) {
                    case TcpOp.StartSend: {
                        TChannel tChannel = this.Get(result.ChannelId);
                        if (tChannel != null) {
                            tChannel.StartSend();
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