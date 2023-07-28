namespace ET.Client {
    [Event(SceneType.Process)] // 作用单位：进程【一个核】。一个进程可以有多个不同的场景。
    public class NetClientComponentOnReadEvent: AEvent<NetClientComponentOnRead> { // 事件 NetClientComponentOnRead 的发出者是：NetClientComponentSystem
        protected override async ETTask Run(Scene scene, NetClientComponentOnRead args) {
            Session session = args.Session;
            object message = args.Message;
            if (message is IResponse response) {  // 【返回消息】：待同步结果到Tcs
                session.OnResponse(response); // 【会话框】上将【返回消息】写入、同步到Tcs 异步任务的结果中去
                return;
            }
            // 【普通消息或者是Rpc请求消息?】：前面我写得对吗？这里说，【网络客户端组件】读到消息事件，接下来，分配到相应【会话框场景】去处理消息 
            MessageDispatcherComponent.Instance.Handle(session, message);
            await ETTask.CompletedTask;
        }
    }
}