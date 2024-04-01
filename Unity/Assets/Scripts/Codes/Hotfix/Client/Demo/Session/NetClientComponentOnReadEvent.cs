namespace ET.Client {
    [Event(SceneType.Process)]
    public class NetClientComponentOnReadEvent: AEvent<Scene, NetClientComponentOnRead> {
        protected override async ETTask Run(Scene scene, NetClientComponentOnRead args) {
            Session session = args.Session;
            object message = args.Message;
			// 【返回消息】：到当前进程，就是到了目的地，直接往下发，会话框上结果写回去
            if (message is IResponse response) { // 【返回消息】：会话框上，把结果写回去，会话框的管理逻辑 RpcInfo.Tcs
                session.OnResponse(response);
                return;
            }
            // 普通消息或者是Rpc请求消息：消息派发组件。感觉这里没看懂没看透彻，昨天夜里夜醒一次，今天状态差一点儿
			// 普通消息或者是Rpc请求消息：到达本进程，也是到达了消息的【被请求方】进程，要下发下放到、各司其职的各小服场景，由它们的功能逻辑来处理
            MessageDispatcherComponent.Instance.Handle(session, message);
            await ETTask.CompletedTask;
        }
    }
}