using System;
using System.IO;
namespace ET.Server { // 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
    [FriendOf(typeof(ActorMessageSenderComponent))]
    public static class ActorMessageSenderComponentSystem {
		// ActorMessage 发送、超时自检测机制：相互独立组件（TimerComponent）、功能模块的、组合，也能合并添加出、相对强大的逻辑——框架里，多个超时自检测自抛超时异常的、自动过滤机制
		[Invoke(TimerInvokeType.ActorMessageSenderChecker)]
        public class ActorMessageSenderChecker: ATimer<ActorMessageSenderComponent> {
            protected override void Run(ActorMessageSenderComponent self) {
                try {
                    self.Check();
                }
                catch (Exception e) {
                    Log.Error($"move timer error: {self.Id}\n{e}");
                }
            }
        }
        [ObjectSystem]
        public class ActorMessageSenderComponentAwakeSystem: AwakeSystem<ActorMessageSenderComponent> {
            protected override void Awake(ActorMessageSenderComponent self) {
                ActorMessageSenderComponent.Instance = self;
				// 每秒钟，就来1 次？
                self.TimeoutCheckTimer = TimerComponent.Instance.NewRepeatedTimer(1000, TimerInvokeType.ActorMessageSenderChecker, self);
            }
        }
        [ObjectSystem]
        public class ActorMessageSenderComponentDestroySystem: DestroySystem<ActorMessageSenderComponent> {
            protected override void Destroy(ActorMessageSenderComponent self) {
                ActorMessageSenderComponent.Instance = null;
                TimerComponent.Instance?.Remove(ref self.TimeoutCheckTimer);
                self.TimeoutCheckTimer = 0;
                self.TimeoutActorMessageSenders.Clear();
            }
        }
        private static void Run(ActorMessageSender self, IActorResponse response) {
            if (response.Error == ErrorCore.ERR_ActorTimeout) { 
                self.Tcs.SetException(new Exception($"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: actorId: {self.ActorId} {self.Request}, response: {response}"));
                return;
            }
            if (self.NeedException && ErrorCore.IsRpcNeedThrowException(response.Error)) {
                self.Tcs.SetException(new Exception($"Rpc error: actorId: {self.ActorId} request: {self.Request}, response: {response}"));
                return;
            }
// 现在ETTask 大部分逻辑都懂，但仍然不懂：内部状态机的运行逻辑，还仍然不懂【TODO】： tcs结果写好，调用方Send().tcs-return 的过程不懂
            self.Tcs.SetResult(response); 
        }
        private static void Check(this ActorMessageSenderComponent self) {
            long timeNow = TimeHelper.ServerNow();
            foreach ((int key, ActorMessageSender value) in self.requestCallback) {
                // 因为是顺序发送的，所以，检测到第一个不超时的就退出
                if (timeNow < value.CreateTime + ActorMessageSenderComponent.TIMEOUT_TIME) {
                    break;
                }
                self.TimeoutActorMessageSenders.Add(key);
            }
            foreach (int rpcId in self.TimeoutActorMessageSenders) {
                ActorMessageSender actorMessageSender = self.requestCallback[rpcId]; // 去找：Send() 消息时，添加到管理字典的地方
                self.requestCallback.Remove(rpcId);
                try {
                    IActorResponse response = ActorHelper.CreateResponse(actorMessageSender.Request, ErrorCore.ERR_ActorTimeout);
					// 超时自检测机制：下面的函数，只走函数 Run() 定义里，异步任务，设置异常的2 个分支
                    Run(actorMessageSender, response);
                }
                catch (Exception e) {
                    Log.Error(e.ToString());
                }
            }
            self.TimeoutActorMessageSenders.Clear();
        }
		// 发送IMessage: IMessage 是不需要回复消息的。并且，这里的 actorId 是【消息的、发送者、发送代理】的、实例身份证号
        public static void Send(this ActorMessageSenderComponent self, long actorId, IMessage message) {
            if (actorId == 0) {
                throw new Exception($"actor id is 0: {message}");
            }
            ProcessActorId processActorId = new(actorId);
            // 这里做了优化，如果发向同一个进程，则等一帧直接处理，不需要通过网络层
            if (processActorId.Process == Options.Instance.Process) { // 同一进程、同一核：可以有多个不同的场景。原本根本就不走网络层呀。。
                async ETTask HandleMessageInNextFrame() {
                    await TimerComponent.Instance.WaitFrameAsync(); // 等1 秒钟：等到当前桢结束，下一桢执行
					// 【服务端】同一进程、同一核：可以有多个不同的场景：【服务端、任何、物理机AppType.Server】：一定添加了NetInnerComponent 组件。
					// 所以同一进程：【IMessage 消息的、接收者进程】是本进程，就是传向本进程、任何可能的场景的消息，走【内网组件】处理消息；
					// 消息目标为【本进程的、任何可能的场景】：走【内网消息组件】——内网组件，添加在任何【服务端、任何、进程】。
					// 所以由，消息目标——本进程的、内网组件处理
					// 消息目标为【本进程的、任何可能的场景】：走【内网消息组件】、这里的【短路——快进、操作】是：
					// 人为手动，帮助逻辑优化，跳过不必要的【网络层、画蛇添足多绕一圈，什么发送、读到消息OnRead() 之类的】，短路调用，要内网组件：【发送、内网收到消息事件】
                    NetInnerComponent.Instance.HandleMessage(actorId, message); // 处理逻辑：热更新域里，发布内网读到消息事件；订阅者去处理逻辑
                }
                HandleMessageInNextFrame().Coroutine();
                return;
            }
			// 不同进程：【TODO】：
            Session session = NetInnerComponent.Instance.Get(processActorId.Process); // 不同进程，哪怕同一物理机，还有什么端口问题。。
            session.Send(processActorId.ActorId, message);
        }
        public static int GetRpcId(this ActorMessageSenderComponent self) {
            return ++self.RpcId; // 自增变量：标记，这个【进程】上的、ActorMessageSender 实例号？
        }
        public static async ETTask<IActorResponse> Call(
                this ActorMessageSenderComponent self,
                long actorId,
                IActorRequest request,
                bool needException = true
        ) {
// 粒度【进程上】的组件：ActorMessageSenderComponent, 发送【内网消息】，IActorRequest 的 RpcId 都来自本进程上，多如牛毛的，发送者实例号，自增变量
            request.RpcId = self.GetRpcId(); 
            if (actorId == 0) {
                throw new Exception($"actor id is 0: {request}");
            }
            return await self.Call(actorId, request.RpcId, request, needException);
        }
		// 【跨进程位置消息】的发送：逻辑基本都懂了
        public static async ETTask<IActorResponse> Call(
                this ActorMessageSenderComponent self,
                long actorId, // IActorRequest 消息的、【接收者进程 actorId】
                int rpcId,    // IActorRequest 消息的、【发送者进程、本进程发送者实例的 actorId】 
                IActorRequest iActorRequest,
                bool needException = true
        ) {
            if (actorId == 0) {
                throw new Exception($"actor id is 0: {iActorRequest}");
            }
            var tcs = ETTask<IActorResponse>.Create(true);
// 封装：1 个异步任务 tcs 进ActorMessageSender
            self.requestCallback.Add(rpcId, new ActorMessageSender(actorId, iActorRequest, tcs, needException)); 
            self.Send(actorId, iActorRequest); // 跨进程位置消息，发出去
            long beginTime = TimeHelper.ServerFrameTime();
            IActorResponse response = await tcs; // 异步返回
            long endTime = TimeHelper.ServerFrameTime();
            long costTime = endTime - beginTime;
            if (costTime > 200) {
                Log.Warning($"actor rpc time > 200: {costTime} {iActorRequest}");
            }
            return response;
        }
        public static void HandleIActorResponse(this ActorMessageSenderComponent self, IActorResponse response) {
            ActorMessageSender actorMessageSender;
            if (!self.requestCallback.TryGetValue(response.RpcId, out actorMessageSender)) {
                return;
            }
            self.requestCallback.Remove(response.RpcId);
            Run(actorMessageSender, response);
        }
    }
}