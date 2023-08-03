namespace ET.Server {
    // 把这个文件；需要同框架原始文件对比一下，感觉被自己改乱了，今天就先看别处。。
    // 为什么Realm 注册登录服，与Gate 网关服里【服务端】组件发布的事情，会有这个场景的订阅者接收事件？
    // 【SceneType.Process】：需要特殊理解，极为特殊的进程场景。它是每个核每个进程必备的一个特殊场景吗？是。Root 单根，首先启动进程场景。为同进程下添加任何其它场景打下座基。
    [Event(SceneType.Process)]  // 【进程】场景？：来处理这个服务端组件事件？外网组件添加的地方是在：【Realm 注册登录服】与【网关服】。是自己写错了？
    public class NetServerComponentOnReadEvent: AEvent<NetServerComponentOnRead> {

        protected override async ETTask Run(Scene scene, NetServerComponentOnRead args) {
            Session session = args.Session;
            object message = args.Message;
            // 【服务端上，会话框】Session: 收到回复消息，会去处理【会话框】上字典管理的回调，将回调的Tcs 异步结果写好。写好了,即刻异步结果到消息请求方
            if (message is IResponse response) { // 到达本进程的【返回消息】: 本进程上将结果写回去，狠简单
                // 借由Tcs 异步，会话框上会同步【返回消息】的内容到Tcs 异步任务的结果；Tcs 任务结果一旦写好，消息请求方就能收到结果
                session.OnResponse(response); 
                return; 
            } 
            // 根据消息接口判断是不是Actor消息，不同的接口做不同的处理,比如需要转发给Chat Scene，可以做一个IChatMessage接口
            switch (message) { // 【发送消息】＋【不要求回复的消息】
                // 【ActorLocationSenderComponent】：先把这一两个组件逻辑给理顺了
                case IActorLocationRequest actorLocationRequest: { // gate session收到actor rpc消息，先向actor 发送rpc请求，再将请求结果返回客户端【原标注】 
                    long unitId = session.GetComponent<SessionPlayerComponent>().PlayerId;
                    int rpcId = actorLocationRequest.RpcId; // 这里要保存客户端的rpcId 【源】: 为什么它说是【客户端】的，不该是被请求消息的人的 rpcId 吗？（被请求的对象的 actorId, 应该是写在发送消息内容里面的）刚才看IMLocationHandler 实现类的封装里，这个作过一遍，这里又作过一遍？？？【这个晚点儿再看】
                    long instanceId = session.InstanceId;
// 【rpcId】 vs 【unitId】：【被】要位置的两方？unitId 是请求位置消息的会话框是玩家的标记号，是位置请求者
                    IResponse iResponse = await ActorLocationSenderComponent.Instance.Call(unitId, actorLocationRequest); // 要【返回消息】的：异步返回【返回消息】，等待至异步结果返回来
                    iResponse.RpcId = rpcId; // 【发送消息】与【返回消息】的 rpcId 是一样的。可是这里的设置，感觉狠奇怪。【位置服】是怎么处理的，这里为什么还得写？
                    // session可能已经断开了，所以这里需要判断【源】。消息类型是【跨进程位置消息】，必须把返回消息【发】回去，是需要发送的
                    if (session.InstanceId == instanceId) 
                        session.Send(iResponse);
                    break;
                }
                case IActorLocationMessage actorLocationMessage: { // 【普通，不要求回复的位置消息】，不是位置回复消息，使用场景？没明白上下两者的区别
                    long unitId = session.GetComponent<SessionPlayerComponent>().PlayerId; // 组件添加的时间：登录【网关服】的时候，网关服会对登录它那里的玩家进行管理
                    // 不需要返回的消息：就直接把消息发出去。可是什么时候会出现这种情况？并且，ActorLocationSenderComponent 组件里，Send() 的明明是【发送消息 IActorRequest】，不是普通消息，感觉这里还有编译错误，这里不通，要回去检查！！！
                    ActorLocationSenderComponent.Instance.Send(unitId, actorLocationMessage); // 组件里，感觉方法不能，回去检查编译错误
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