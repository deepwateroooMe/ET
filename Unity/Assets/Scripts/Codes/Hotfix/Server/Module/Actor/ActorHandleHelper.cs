using System;
namespace ET.Server {
    public static class ActorHandleHelper {
        // 去回想IMHandler 接口的两个抽象实现类：如何发返回消息的；这里帮助类，也封装一个发返回消息的方法 
        public static void Reply(int fromProcess, IActorResponse response) {
            if (fromProcess == Options.Instance.Process) { // 返回消息是同一个进程：没明白，这里为什么就断定是同一进程的消息了？直接处理
                // NetInnerComponent.Instance.HandleMessage(realActorId, response); // 等同于直接调用下面这句【我自己暂时放回来的】
                ActorMessageSenderComponent.Instance.HandleIActorResponse(response); // 【没读懂：】同一个进程内的消息，不走网络层，直接处理。什么情况下会是发给同一个进程的？ET7 重构后，同一进程下可能会有不同的先前小服：Realm 注册登录服，Gate 服等；如果不同的SceneType.Map-etc 先前场景小服只要在同一进程，就可以不走网络层吗？
                return;
            }
            // 【不同进程的消息处理：】走网络层，调用会话框来发出消息. 【这会儿，把这个，内网消息，会话框上发返回消息到其它进程，再快看一遍】仍然是有有疑问的地方，改天再看！
            Session replySession = NetInnerComponent.Instance.Get(fromProcess); // 从内网组件单例中去拿会话框：不同进程消息，一定走网络，通过会话框把返回消息发回去
            replySession.Send(response);
        }
        public static void HandleIActorResponse(IActorResponse response) {
            ActorMessageSenderComponent.Instance.HandleIActorResponse(response);
        }
        // 【分发actor消息】：发送消息只看了一半，返回来时，如何处理的？
        [EnableAccessEntiyChild]
        public static async ETTask HandleIActorRequest(long actorId, IActorRequest iActorRequest) {
            InstanceIdStruct instanceIdStruct = new(actorId);
            int fromProcess = instanceIdStruct.Process; // 来自于发送消息方的信息，【发送方进程】
            instanceIdStruct.Process = Options.Instance.Process; // ？？？感觉这里是，重新封装，因为返回消息，也会先再回到这里来再重新封装？要把这整个过程看完
            long realActorId = instanceIdStruct.ToLong();
            Entity entity = Root.Instance.Get(realActorId); // 发送消息发送方的实体，与发送邮箱。（这里需要再检查，感觉Root 根场景出来得奇怪，谁是在管理这些 realActorId 呢？）
            // 【上面】：上面好几行，全只是为了拿到发送消息小伙伴的实倒 instanceId 吗？感觉这个过程，与原理，没能理解透彻。。。不明白  
            if (entity == null) { // 出错时，框架底层自动封装一个异常出错返回消息，用于抛异常，以前看过了
                IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                Reply(fromProcess, response);
                return;
            }
            // 先把这里，【MailBoxComponent 组件】，基本上是区分【服务端场景的无序消息分发器】，与【客户端】场景等的收发邮件功能 
            MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
            if (mailBoxComponent == null) {
                Log.Warning($"actor not found mailbox: {entity.GetType().Name} {realActorId} {iActorRequest}");
                IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                Reply(fromProcess, response);
                return;
            }
            switch (mailBoxComponent.MailboxType) { // 这里区分：发送消息者，是一个场景（用来收发转发【跨进程消息】），还是一个玩家实体（收发【同进程】和或【不同进程】的消息）？
            case MailboxType.MessageDispatcher: { // 【单线程多进程框架】，同一时间可能存在多个进程同时索要【发送消息的实体 realActorId】, 务必加锁，甚至站队排列等并发的次序
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, realActorId)) {
                        if (entity.InstanceId != realActorId) { // 这里检查：【发送消息的实体】小伙伴 me 正在搬家，搬家要花两整天，这两天内位置位移变化不确定，这两天里业务暂停。暂不收发消息。。。
                            IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor); // 抛异常：发送消息的实体掉线了、下线了，不能发消息
                            Reply(fromProcess, response);
                            break;
                        } // 调用管理器组件的处理方法
                        // 同【内网组件NetInnerComponent】一样，ActorMessageDispatcherComponent 也是作用于进程上。从一进程下来，先调用进程上的【消息分发器】进行处理
                        await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorRequest);
                    }
                    break;
                }
// 在场景工厂【SceneFactory】的【创建服务端场景】CreateServerScene() 方法里添加的：这个【服务端无序消息转发器】。任何【服务端场景】都有
// 不用上锁，不用抛异常，使用场景上下文？【服务端场景】，比如【网关服】，可以直接转发消息呀，是诸多【客户端】的代理（转发给其它各服），也是各【服务端】的下传客户端转发中介
            case MailboxType.UnOrderMessageDispatcher: { 
                    await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorRequest);
                    break;
                }
            case MailboxType.GateSession: // 【不明白：】既把【网关服】独立出来了，又不能怎么样，这里到底要【网关服】怎么样呢？它不是中介吗？
                default:
                    throw new Exception($"no mailboxtype: {mailBoxComponent.MailboxType} {iActorRequest}");
            }
        }
        // 分发actor消息
        [EnableAccessEntiyChild]
        public static async ETTask HandleIActorMessage(long actorId, IActorMessage iActorMessage) {
            InstanceIdStruct instanceIdStruct = new(actorId);
            int fromProcess = instanceIdStruct.Process;
            instanceIdStruct.Process = Options.Instance.Process;
            long realActorId = instanceIdStruct.ToLong();
            Entity entity = Root.Instance.Get(realActorId);
            if (entity == null) {
                Log.Error($"not found actor: {realActorId} {iActorMessage}");
                return;
            }
            MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
            if (mailBoxComponent == null) {
                Log.Error($"actor not found mailbox: {entity.GetType().Name} {realActorId} {iActorMessage}");
                return;
            }
            switch (mailBoxComponent.MailboxType) {
                case MailboxType.MessageDispatcher: {
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, realActorId)) {
                        if (entity.InstanceId != realActorId) 
                            break;
                        await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorMessage);
                    }
                    break;
                }
                case MailboxType.UnOrderMessageDispatcher: {
                    await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorMessage);
                    break;
                }
// 【网关服】中介：【网关服】中介帮助【客户端】，转发【客户端】消息到收消息实体所在的【服务端】？？？写反了，【网关服】收到跨进程消息后，帮直接转发给【客户端】
                    // 这里，这个类，【跨进程消息处理器】，能够到达【网关服】的消息，都是来自于其它服的【跨进程消息】，所以是转给【客户端】
            case MailboxType.GateSession: { 
                    if (entity is Session gateSession) 
                        // 发送给客户端
                        gateSession.Send(iActorMessage); // 【会话框】上发消息：走底层网络层，跨进程消息发送【网关服】所管理的小区下的所有客户端，会是在同一进程上的吗？呵呵呵。。
                    break; 
                }
                default:
                    throw new Exception($"no mailboxtype: {mailBoxComponent.MailboxType} {iActorMessage}");
            }
        }
    }
}
