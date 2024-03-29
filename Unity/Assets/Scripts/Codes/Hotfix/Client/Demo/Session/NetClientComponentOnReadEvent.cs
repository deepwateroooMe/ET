namespace ET.Client {
    [Event(SceneType.Process)] // 作用单位：进程【一个核】。一个进程可以有多个不同的场景。
    public class NetClientComponentOnReadEvent: AEvent<NetClientComponentOnRead> { // 事件 NetClientComponentOnRead 的发出者是：NetClientComponentSystem
		// 区分：【网络客户端组件、读到消息事件】：读到的消息，可以是【返回消息】到客户端；也可以是【跨进程、请求消息】需要回复，区别对待
        protected override async ETTask Run(Scene scene, NetClientComponentOnRead args) {
            Session session = args.Session;
            object message = args.Message;
			// 【返回消息】：这个过程忘记了，明天早上再看一下
            if (message is IResponse response) {  // 【返回消息】：待同步结果到Tcs
                session.OnResponse(response); // 【会话框】上将【返回消息】写入、同步到Tcs 异步任务的结果中去
                return;
            }
            // 【普通消息？、或者是Rpc请求消息?】普通消息？
			// 返方向找：有多少不同使用场景下，发布了这个 NetClientComponentOnReadEvent 事件？网络客户端读到消息事件，框架里，仅只【网络客户端组件】发布过此事件
            MessageDispatcherComponent.Instance.Handle(session, message); // 先找：挂载这个组件的、几种类型＝＝》【双端都有】
			// 上面：同一进程上的、单例【消息派发器组件】，负责将，这类消息，派发到、对应场景固定场景下的、消息处理器，去处理 
            await ETTask.CompletedTask; // 这里，也想再看一下【TODO】：
        }
    }
}