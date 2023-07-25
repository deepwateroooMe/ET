using System;
namespace ET.Server {
    [Event(SceneType.Process)] // 这里，为什么事件的【订阅者】，是在进程上？ 内网组件，是添加在什么场景上的？AppType.Server 上添加【内网组件】，应该是进程上的吧
    public class NetInnerComponentOnReadEvent: AEvent<NetInnerComponentOnRead> {
        protected override async ETTask Run(Scene scene, NetInnerComponentOnRead args) {
            try {
                long actorId = args.ActorId;
                object message = args.Message;
                // 收到actor消息,放入actor队列
                switch (message) { // 分不同的消息类型，借助 ActorHandleHelper 帮助类，对消息进行处理。既处理【请求消息】，也处理【返回消息】，还【普通消息】
                case IActorResponse iActorResponse: { // 对【内网组件读到】【返回消息】的处理逻辑：今天晚上，终于看懂连通了
                        ActorHandleHelper.HandleIActorResponse(iActorResponse);
                        break;
                    }
// 【亲爱的表哥，现在是，一年中活宝妹的本尊（狮子月）！！】这个月，从今天晚上开始，活宝妹都要尽力好好学习了！！一年 12 个月，只有活宝妹的狮子月，活宝妹的理解力、学习效率最高！！！
// 订个大致的小计划：
    // 搬家前的五天，把框架，如读【协程锁】如读今天晚上，以前看不懂的地方【网络模块】【Actor 模块】【Handler?】【动态路由模块】今晚能懂般，把它们都看懂，作好笔记；
    // 周日周一尽量只下午傍晚去搬，两个下午三四次应该能够搬完。上午和晚上的时间，仍然需要好好学习
    // 8/1/2023: 搬家后，需要尽快适应新环境，找到早上尽早来到学校后，傍晚才回家的午餐不致病健康午餐饮食。如果每天一定需要呆家里一会儿，尽量留在晚上，傍晚弄点儿吃的，晚上家里再学习会儿，保障白天呆学校
// 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
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