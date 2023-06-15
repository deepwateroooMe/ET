using System;
namespace ET.Server {
    // 这个，应该是服务端发布读事件后，触发的订阅者处理读到消息的回调逻辑：分消息类型，进行不同的处理
    [Event(SceneType.Process)]
    public class NetInnerComponentOnReadEvent: AEvent<NetInnerComponentOnRead> {
        protected override async ETTask Run(Scene scene, NetInnerComponentOnRead args) {
            try {
                long actorId = args.ActorId;
                object message = args.Message;
                // 收到actor消息,放入actor队列
                switch (message) { // 分不同的消息类型，借助 ActorHandleHelper 帮助类，对消息进行处理。既处理【请求消息】，也处理【返回消息】，还【普通消息】
                    case IActorResponse iActorResponse: {
                        ActorHandleHelper.HandleIActorResponse(iActorResponse);
                        break;
                    }
                    case IActorRequest iActorRequest: {
                        await ActorHandleHelper.HandleIActorRequest(actorId, iActorRequest);
                        break;
                    }
                    case IActorMessage iActorMessage: {
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
}