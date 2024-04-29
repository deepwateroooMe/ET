namespace ET.Client {
	
	// 粒度单位：【进程】。进程，就是对一台物理机一个进程上，所有场景的总管。。
    [Event(SceneType.Process)]
    public class NetClientComponentOnReadEvent: AEvent<Scene, NetClientComponentOnRead> {
        protected override async ETTask Run(Scene scene, NetClientComponentOnRead args) {
            Session session = args.Session;
            object message = args.Message;
			// 【返回消息】：到当前进程，就是到了目的地，直接往下发，会话框上结果写？回去。是回给客户端的
            if (message is IResponse response) { // 【返回消息】：会话框上，把结果写回去，会话框的管理逻辑 RpcInfo.Tcs
                session.OnResponse(response);    // 【返回消息】极简单：收消息【客户端】在本进程上，【会话框】上把消息结果写回去 Tcs.SetResult(response) 就可以了
                return;
            }
			// 除了IResponse 之外，就剩下两2 种： IMessage IRequest
            // 普通消息或者是Rpc请求消息：消息派发组件
			// 普通消息或者是Rpc请求消息：到达本进程，也是到达了消息的【被请求方】进程，要下发下放到、各司其职的各小服场景，由它们的功能逻辑来处理
			// 【TODO】：普通消息或者是Rpc请求消息【源】；【普通消息】就是IRequest IMessage, 【Rpc 请求消息】是IActorRequest IActorMessage.
            MessageDispatcherComponent.Instance.Handle(session, message); // 处理逻辑：下发给具体的场景，让场景里的消息处理器去处理
            await ETTask.CompletedTask;
        }
    }
}