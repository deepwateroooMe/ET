using System;
namespace ET.Server {
    public static class ActorHandleHelper {
        public static void Reply(int fromProcess, IActorResponse response) {
            if (fromProcess == Options.Instance.Process) { // 返回消息是同一个进程：没明白，这里为什么就断定是同一进程的消息了？直接处理
                // NetInnerComponent.Instance.HandleMessage(realActorId, response); // 等同于直接调用下面这句【我自己暂时放回来的】
                ActorMessageSenderComponent.Instance.HandleIActorResponse(response); // 【没读懂：】同一个进程内的消息，不走网络层，直接处理。什么情况下会是发给同一个进程的？ET7 重构后，同一进程下可能会有不同的先前小服：Realm 注册登录服，Gate 服等；如果不同的SceneType.Map-etc 先前场景小服只要在同一进程，就可以不走网络层吗？
                return;
            }
            // 【不同进程的消息处理：】走网络层，就是调用会话框来发出消息
            Session replySession = NetInnerComponent.Instance.Get(fromProcess); // 从内网组件单例中去拿会话框：不同进程消息，一定走网络，通过会话框把返回消息发回去
            replySession.Send(response);
        }
        public static void HandleIActorResponse(IActorResponse response) {
            ActorMessageSenderComponent.Instance.HandleIActorResponse(response);
        }
        // 分发actor消息
        [EnableAccessEntiyChild]
        public static async ETTask HandleIActorRequest(long actorId, IActorRequest iActorRequest) {
            InstanceIdStruct instanceIdStruct = new(actorId);
            int fromProcess = instanceIdStruct.Process;
            instanceIdStruct.Process = Options.Instance.Process;
            long realActorId = instanceIdStruct.ToLong();
            Entity entity = Root.Instance.Get(realActorId);
            if (entity == null) {
                IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                Reply(fromProcess, response);
                return;
            }
            MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
            if (mailBoxComponent == null) {
                Log.Warning($"actor not found mailbox: {entity.GetType().Name} {realActorId} {iActorRequest}");
                IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                Reply(fromProcess, response);
                return;
            }
            switch (mailBoxComponent.MailboxType) {
                case MailboxType.MessageDispatcher: {
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, realActorId)) {
                        if (entity.InstanceId != realActorId) {
                            IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                            Reply(fromProcess, response);
                            break;
                        } // 调用管理器组件的处理方法 
                        await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorRequest);
                    }
                    break;
                }
                case MailboxType.UnOrderMessageDispatcher: {
                    await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorRequest);
                    break;
                }
                case MailboxType.GateSession:
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
                case MailboxType.GateSession: {
                    if (entity is Session gateSession) 
                        // 发送给客户端
                        gateSession.Send(iActorMessage);
                    break;
                }
                default:
                    throw new Exception($"no mailboxtype: {mailBoxComponent.MailboxType} {iActorMessage}");
            }
        }
    }
}

