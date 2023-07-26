namespace ET.Server {
    // 为什么Realm 注册登录服，与Gate 网关服里【服务端】组件发布的事情，会有这个场景的订阅者接收事件？内网间这种消息，不是狠多吗？源作者说【客户端】的发送消息是狠少的。。。
    [Event(SceneType.Process)]  // 【进程】场景？：来处理这个服务端组件事件？外网组件添加的地方是在：【Realm 注册登录服】与【网关服】。是自己写错了？
    public class NetServerComponentOnReadEvent: AEvent<NetServerComponentOnRead> {
        protected override async ETTask Run(Scene scene, NetServerComponentOnRead args) { // 【返回消息的发送】：仍是封装在框架底层
            Session session = args.Session;
            object message = args.Message;
// 【回复消息】：前面的重点看错了，重点是【回复】消息，能够到达当前服务端组件，就说明是属于当前进程收的消息，就是内网消息到达【网关服】，【网关服】下发给【客户端】
            // 所以不区分IRpcResponse IRpcLocationResponse 类型, 都向下交由会话框处理
            // 【服务端上，会话框】Session: 发，还是不发，消息到Channel 的另一头
            if (message is IResponse response) { // 【回复消息】: 就去服务端，什么情况下会出现这种情况？
                // 【没读懂】：服务端组件，处理回复消息，要不要【发】返回消息的步骤？找不到哪里有，发送，的这个步骤。
                // 而会话框，若真是从其它服转过来的消息，现服务端不已经是会话框的另一端了吗（内网消息也走会话框吗？还得接着去读内网Inner.）？感觉这里哪里没懂。。。
                session.OnResponse(response); // 【会话框】：直接处理返回消息。处理的逻辑也仅限于将RpcInfo.Tcs 异步任务的返回结果写好。并不负责将返回消息发送回去。
                return; // 这里返回：我仍然找不到，返回消息是，如何【发送】回去的？发送的过程步骤调用，是哪里处理的？
            } // 对于【服务端】来说，【会话框】上，只要把【异步任务TCS】的结果写好填好，异步网络Session 底层，会自动处理Channel 客户端那一头的自动读取，框架是不用管的？
            // 根据消息接口判断是不是Actor消息，不同的接口做不同的处理,比如需要转发给Chat Scene，可以做一个IChatMessage接口
            switch (message) { // 【发送消息】＋【不要求回复的消息】
                // 【下面的注释：】应该是原框架的人写的。那么参考这里，也就是说，虽然SceneType.Process, 但仍存在网关服场景下的处理情况？
                // 【ActorLocationSenderComponent】：先把这一两个组件逻辑给理顺了
                case IActorLocationRequest actorLocationRequest: { // gate session收到actor rpc消息，先向actor 发送rpc请求，再将请求结果返回客户端【原标注】 
                    long unitId = session.GetComponent<SessionPlayerComponent>().PlayerId;
                    int rpcId = actorLocationRequest.RpcId; // 这里要保存客户端的rpcId
                    long instanceId = session.InstanceId;
                    IResponse iResponse = await ActorLocationSenderComponent.Instance.Call(unitId, actorLocationRequest);
                    iResponse.RpcId = rpcId; // 【发送消息】与【返回消息】的 rpcId 是一样的
                    // session可能已经断开了，所以这里需要判断
                    if (session.InstanceId == instanceId) 
                        session.Send(iResponse);
                    break;
                }
                case IActorLocationMessage actorLocationMessage: { // 【普通，不要求回复的位置消息】
                    long unitId = session.GetComponent<SessionPlayerComponent>().PlayerId;
                    ActorLocationSenderComponent.Instance.Send(unitId, actorLocationMessage); // 把这里发送位置消息再看一遍，快速看一遍，总记不住
                    break;
                }
                case IActorRequest actorRequest:  // 分发IActorRequest消息，目前没有用到，需要的自己添加 
                    break;
                case IActorMessage actorMessage:  // 分发IActorMessage消息，目前没有用到，需要的自己添加 
                    break;
            default: { // 非Actor消息的话：应该就是本进程消息，不走网络层，进程内处理
                    // 非Actor消息： MessageDispatcherComponent 全局单例吗？是的
                    MessageDispatcherComponent.Instance.Handle(session, message);
                    break;
                }
            }
        }
    }
}