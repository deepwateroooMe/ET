namespace ET.Server {
    // 为什么Realm 注册登录服，与Gate 网关服里【服务端】组件发布的事情，会有这个场景的订阅者接收事件？
    // 【SceneType.Process】：需要特殊理解，极为特殊的进程场景。它是每个核每个进程必备的一个特殊场景吗？是。Root 单根，首先启动进程场景。为同进程下添加任何其它场景打下座基。
    [Event(SceneType.Process)]  // 【进程】场景？：来处理这个服务端组件事件？外网组件添加的地方是在：【Realm 注册登录服】与【网关服】。是自己写错了？
    public class NetServerComponentOnReadEvent: AEvent<NetServerComponentOnRead> {

        protected override async ETTask Run(Scene scene, NetServerComponentOnRead args) {
            Session session = args.Session;
            object message = args.Message;
            // 【服务端上，会话框】Session: 收到回复消息，会去处理【会话框】上字典管理的回调，将回调的Tcs 异步结果写好。写好了,即刻异步结果到消息请求方
            if (message is IResponse response) { // 【返回消息】: 借由Tcs 异步，会话框上会同步【返回消息】的内容到Tcs 异步任务的结果；Tcs 任务结果一旦写好，消息请求方就能收到结果
                session.OnResponse(response); 
                return; 
            } 
            // 【今天看了一天这个文件】：line 25 行，ActorLocationSenderComponent 组件里，发消息时，锁的是【发送位置消息】的，还是【被查询位置消息】的，仍然没看懂，更像是发送索要位置消息的进程 ActorId 。。昏昏。。
            // 根据消息接口判断是不是Actor消息，不同的接口做不同的处理,比如需要转发给Chat Scene，可以做一个IChatMessage接口
            switch (message) { // 【发送消息】＋【不要求回复的消息】
                // 【下面的注释：】应该是原框架的人写的。那么参考这里，也就是说，虽然SceneType.Process, 但仍存在网关服场景下的处理情况？
                // 【ActorLocationSenderComponent】：先把这一两个组件逻辑给理顺了
                case IActorLocationRequest actorLocationRequest: { // gate session收到actor rpc消息，先向actor 发送rpc请求，再将请求结果返回客户端【原标注】 
                    long unitId = session.GetComponent<SessionPlayerComponent>().PlayerId;
                    int rpcId = actorLocationRequest.RpcId; // 这里要保存客户端的rpcId 
                    long instanceId = session.InstanceId;
                    IResponse iResponse = await ActorLocationSenderComponent.Instance.Call(unitId, actorLocationRequest); // 【rpcId】 vs 【unitId】：【被】要位置的两方，这里仍是糊涂的。。。
                    iResponse.RpcId = rpcId; // 【发送消息】与【返回消息】的 rpcId 是一样的。可是这里的设置，感觉狠奇怪。【位置服】是怎么处理的，这里为什么还得写？
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
                MessageDispatcherComponent.Instance.Handle(session, message); // 下午还有点儿时间，就把这个再看看，还能捡出点儿什么来。。。
                    break;
                }
            }
        }
    }
}