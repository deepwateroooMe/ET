namespace ET.Client {

// 感觉这里是事件触发之后的回调:就是搞不懂，什么情况下会触发出执行这个回调方法 ?    
    [Event(SceneType.Process)]
    public class NetClientComponentOnReadEvent: AEvent<NetClientComponentOnRead> {

        protected override async ETTask Run(Scene scene, NetClientComponentOnRead args) {
            Session session = args.Session;
            object message = args.Message;
            if (message is IResponse response) {
                session.OnResponse(response);
                return;
            }
            
            // 普通消息或者是Rpc请求消息
            MessageDispatcherComponent.Instance.Handle(session, message);
            await ETTask.CompletedTask;
        }
    }
}