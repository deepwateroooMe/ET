namespace ET.Client {
    [Event(SceneType.Process)]
    public class NetClientComponentOnReadEvent: AEvent<NetClientComponentOnRead> { // 事件 NetClientComponentOnRead 的发出者是：NetClientComponent
        protected override async ETTask Run(Scene scene, NetClientComponentOnRead args) {
            Session session = args.Session;
            object message = args.Message;
// 这个是说：从服务端回来，到达本客户端的回复消息？那么就相应的会话框上回回去，就是更底层【会话框 socket 端口】网络异步信息的本地读取相关，不用看太细太底层了，想明白就可以
            if (message is IResponse response) { 
                session.OnResponse(response); // 今天，这个上次不曾看懂的地方，再看一遍，能多懂多少？
                return;
            }
            // 普通消息或者是Rpc请求消息（这里自己去想：客户端可以有自己，想要发送出去的消息，同样会发布事件，触发此回调吗？去翻框架）
            MessageDispatcherComponent.Instance.Handle(session, message);
            await ETTask.CompletedTask;
        }
    }
}