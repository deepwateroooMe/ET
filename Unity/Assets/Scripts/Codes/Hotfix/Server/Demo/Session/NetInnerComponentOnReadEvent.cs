using System;
namespace ET.Server {
    [Event(SceneType.Process)] // 事件机制、粒度单位：核进程。快速把这个类三个分支看完
    public class NetInnerComponentOnReadEvent: AEvent<Scene, NetInnerComponentOnRead> {
        protected override async ETTask Run(Scene scene, NetInnerComponentOnRead args) {
            try {
                long actorId = args.ActorId;
                object message = args.Message;
                
                // 收到actor消息,放入actor队列
                switch (message) {
					case IActorResponse iActorResponse: {
						// Actor 消息发送时，巧妙封装 ETTask<IActorResponse> tcs 成员变量，是返回消息，就把结果写回去就可以了
						// 【TODO】：ETTask 底层原理、状态机、各种状态，如SetResult() 后的回调等，感觉不透彻
                        ActorHandleHelper.HandleIActorResponse(iActorResponse);
                        break;
                    }
					// 这个比较流程化：感觉都看懂了
					case IActorRequest iActorRequest: {
                        await ActorHandleHelper.HandleIActorRequest(actorId, iActorRequest);
                        break;
                    }
					// 【普通Actor 消息】：【TODO】：里面还有细节疑问，如 actor 消息处理器里ResponseType 的处理，慢慢弄明白 
					case IActorMessage iActorMessage: { // 普通IActorMessage: 区分极清楚 
                        await ActorHandleHelper.HandleIActorMessage(actorId, iActorMessage);
                        break;
                    }
                }
            }
            catch (Exception e) {
                Log.Error($"InnerMessageDispatcher error: {args.Message.GetType().Name}\n{e}");
            }
            await ETTask.CompletedTask;
        }
    }
} // 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！