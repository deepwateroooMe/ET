using System;
namespace ET.Server {
    [Event(SceneType.Process)] // 事件的【订阅者】，是在进程上？ 内网组件，是添加在什么场景上的？AppType.Server 上添加【内网组件】，所以回调事件，也作用在进程上
    public class NetInnerComponentOnReadEvent: AEvent<NetInnerComponentOnRead> {
        protected override async ETTask Run(Scene scene, NetInnerComponentOnRead args) {
            try {
                long actorId = args.ActorId;
                object message = args.Message;
                // 收到actor消息,放入actor队列
// 分不同的消息类型，借助 ActorHandleHelper 帮助类，对消息进行处理。既处理【请求消息】，也处理【返回消息】，还【普通消息】。这里我从同一进程上消息处理跟下来，应该也适用跨进程消息
                switch (message) { 
                case IActorResponse iActorResponse: { // 对【内网组件读到】【返回消息】的处理逻辑：
                    ActorHandleHelper.HandleIActorResponse(iActorResponse); // 【返回消息】：借ActorMessageSender 里封装的桥梁异步任务 Tcs 把结果返回给调用方
                        break;
                    }
                case IActorRequest iActorRequest: { // 【跨进程请求消息】：这个只看了半懂，拿发送消息小伙伴的实例 instanceId 和发送邮箱的过程，没懂
                        await ActorHandleHelper.HandleIActorRequest(actorId, iActorRequest);
                        break;
                    }
                case IActorMessage iActorMessage: { // 这里，跟上面的逻辑，是一样的
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