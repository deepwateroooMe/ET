using System;
namespace ET.Server {
	// 静态帮助类：注意区分：普通Actor 消息，与位置 Actor 往返消息，是否共用这个类？【TODO】：晚点儿、看完位置消息后、会能够确认
	// 静态帮助类ActorHandleHelper: 多少个服务器？进程？，使用这个类？想明白这个，才是下面，单线程【多线程】多进程，访问、某个进程目标场景邮箱、要用协程锁的理由
    public static class ActorHandleHelper {
		// Reply(): 这个过程，感觉读了千百遍了：目标进程为【本进程同进程】就短路不走网络层；否则内网会话框、发内网消息到目标进程上去
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
            long realActorId = instanceIdStruct.ToLong(); // 一目了然的：【目标实例——收件人】 actorId
            Entity entity = Root.Instance.Get(realActorId);
            if (entity == null) {
                IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                Reply(fromProcess, response);
                return;
            }
            MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
            if (mailBoxComponent == null) { // 目标实例：不具体收件功能。给消息请求方，抛个出错通告让它知道 
                Log.Warning($"actor not found mailbox: {entity.GetType().FullName} {realActorId} {iActorRequest}");
                IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                Reply(fromProcess, response); // 即刻回复回去
                return;
            }
            switch (mailBoxComponent.MailboxType) {
				// 2 种类型区别：派发器用协程锁；无序不用协程锁，原因？【TODO】：现在
				case MailboxType.MessageDispatcher: { // Init_Share: 【双端、任何一端、进程上】都挂载这种【消息派发器】：
					// 以【目标实例——收件人】 actorId 为键的、协程锁，为什么，先前的亲爱的表哥的活宝妹，就是傻傻瓣不清楚？现在看多简单？先前的亲爱的表哥的活宝妹，一定是笨哭了。。。
					// 多个处理针对同一个实体的Mailbox消息处理，处理需要按照先后顺序【摘抄自网络】一看就明白的呀。。。先前的亲爱的表哥的活宝妹、笨宝妹；现在的聪明活宝木头妹。。。
                    using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, realActorId)) { // 一目了然的：【目标实例——收件人】 actorId
						// 【协程锁】：多少个服务器？进程？，使用这个静态帮助类？想明白这个，才是这里，单线程【多线程】多进程，访问、某个进程目标场景邮箱、要用协程锁的理由
						// 【协程锁】：简单说，就是【单多】线程多进程服务器环境下，多进程，，访问、某个进程目标场景邮箱、多进程安全，一定要用协程锁！就是如看医生，发送者、病患、要排除挂号！
                        if (entity.InstanceId != realActorId) {
                            IActorResponse response = ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                            Reply(fromProcess, response);
                            break;
                        }
                        await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorRequest);
                    }
                    break;
                }
				case MailboxType.UnOrderMessageDispatcher: { // 无序的：不用协程锁。【服务端、任何场景】都挂载这种派发器
					// 将、到达【本进程】的内网消息，下派发到、各司其职的、司特定功能的、多个备份分身小服，去处理【负责回复消息什么的】
					// 现在，快速看一遍：回复消息回去的过程：底层封装消息的自动回复；自动回复是？【TODO】：现在
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
            InstanceIdStruct instanceIdStruct = new(actorId); // 在现 actorId 的基础上，添加了【进程、身份证号】和【时间信息】
            int fromProcess = instanceIdStruct.Process;
            instanceIdStruct.Process = Options.Instance.Process;
            long realActorId = instanceIdStruct.ToLong(); // 因为又InstanceIdStruct 一层封装，所以不同于 actorId, 但仍是【身份证号——唯一标识】
            Entity entity = Root.Instance.Get(realActorId); // 应该还是拿 actorId 相关信息，去根场景下找实体 entity ——收件实例
            if (entity == null) { // 空：一定报错
                Log.Error($"not found actor: {realActorId} {iActorMessage}");
                return;
            }
            MailBoxComponent mailBoxComponent = entity.GetComponent<MailBoxComponent>();
            if (mailBoxComponent == null) {
				// 【收件人】：当然一定要，具备，可以接收【跨进程 actor 消息】的能力——身挂 MailBoxComponent 组件
                Log.Error($"actor not found mailbox: {entity.GetType().FullName} {realActorId} {iActorMessage}");
                return;
            }
			// 对【目标——收件人】实体的、邮箱类型、分类处理：
            switch (mailBoxComponent.MailboxType) {
				case MailboxType.MessageDispatcher: {
					using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Mailbox, realActorId)) {
						if (entity.InstanceId != realActorId) {
							break;
						}
						await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorMessage);
					}
					break;
				}
			case MailboxType.UnOrderMessageDispatcher: { // 任何【服务端】场景：粒度单位【1 个进程上的、N 多场景、各服务端场景】，都挂载这种邮箱
				// 根据粒度单位，可想而知：
				// 【自底向上，由N 多场景中的某个目标场景 entity 实例，回顶上至这个场景所属进程上的】ActorMessageDispatcherComponent 组件，来负责处理收目标 IactorMsg
					await ActorMessageDispatcherComponent.Instance.Handle(entity, fromProcess, iActorMessage);
					break;
				}
				case MailboxType.GateSession: { // 框架开发者对【邮箱组件】分类型的初衷：网关服直接下发客户端
					if (entity is Player player) {
						player.GetComponent<PlayerSessionComponent>()?.Session?.Send(iActorMessage); // 会话框上发消息，底层细节，老被亲爱的表哥的活宝妹忘记，可是一看就懂
					}
					break;
				}
				default:
					throw new Exception($"no mailboxtype: {mailBoxComponent.MailboxType} {iActorMessage}");
            }
        }
    }
}