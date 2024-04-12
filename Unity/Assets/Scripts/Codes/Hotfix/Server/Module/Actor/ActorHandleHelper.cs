using System;
namespace ET.Server {
	// 静态帮助类：注意区分：普通Actor 消息，与位置 Actor 往返消息，是否共用这个类？【TODO】：晚点儿、看完位置消息后、会能够确认
    public static class ActorHandleHelper {
        public static void Reply(int fromProcess, IActorResponse response) {
            if (fromProcess == Options.Instance.Process) { // 返回消息是同一个进程 
                async ETTask HandleMessageInNextFrame() {
                    await TimerComponent.Instance.WaitFrameAsync();
                    NetInnerComponent.Instance.HandleMessage(0, response);
                }
                HandleMessageInNextFrame().Coroutine();
                return;
            }
            Session replySession = NetInnerComponent.Instance.Get(fromProcess);
            replySession.Send(response);
        }
		// IActorResponse: 当IActorRequest 消息 Send() 时，曾加入 ActorMessageSenderComponent 组件，对其回调【返回消息】加入字典管理
		// 现在，回复消息 ready, 仍走管理组件字典，去回调——将封装的异步任务、写结果到【消息发送时、提供的消息索引地址地方】
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
                Log.Warning($"actor not found mailbox: {entity.GetType().FullName} {realActorId} {iActorRequest}");
                IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                Reply(fromProcess, response);
                return;
            }
            switch (mailBoxComponent.MailboxType) {
                case MailboxType.MessageDispatcher: {
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, realActorId)) {
                        if (entity.InstanceId != realActorId)
                        {
                            IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                            Reply(fromProcess, response);
                            break;
                        }
                        await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorRequest);
                    }
                    break;
                }
                case MailboxType.UnOrderMessageDispatcher: {
                    await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorRequest);
                    break;
                }
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
                Log.Error($"actor not found mailbox: {entity.GetType().FullName} {realActorId} {iActorMessage}");
                return;
            }
            switch (mailBoxComponent.MailboxType) {
                case MailboxType.MessageDispatcher: {
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, realActorId)) {
                        if (entity.InstanceId != realActorId)
                        {
                            break;
                        }
                        await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorMessage);
                    }
                    break;
                }
                case MailboxType.UnOrderMessageDispatcher: {
                    await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorMessage);
                    break;
                }
                case MailboxType.GateSession: {
                    if (entity is Player player) {
                        player.GetComponent<PlayerSessionComponent>()?.Session?.Send(iActorMessage);
                    }
                    break;
                }
                default:
                    throw new Exception($"no mailboxtype: {mailBoxComponent.MailboxType} {iActorMessage}");
            }
        }
    }
}