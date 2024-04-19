using System;
using System.IO;
using MongoDB.Bson;
namespace ET.Server {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	// 这个类，感觉大部分都看懂了；可是框架里的实际用例，真正调用的地方，可能得找几个实例再看一下，才算真正看懂。
	// 【超时机制】：几个不同的超时异常，没看懂
	[FriendOf(typeof(ActorLocationSenderOneType))]
    [FriendOf(typeof(ActorLocationSender))]
    public static class ActorLocationSenderComponentSystem {
        [Invoke(TimerInvokeType.ActorLocationSenderChecker)] // 下面的重复闹钟：闹的时候，就会调用下面的 Run() 回调
        public class ActorLocationSenderChecker: ATimer<ActorLocationSenderOneType> {
            protected override void Run(ActorLocationSenderOneType self) {
                try {
                    self.Check();
                }
                catch (Exception e) {
                    Log.Error($"move timer error: {self.Id}\n{e}");
                }
            }
        }
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<ActorLocationSenderOneType, int> {
            protected override void Awake(ActorLocationSenderOneType self, int locationType) {
                self.LocationType = locationType;
                // 每10s扫描一次过期的actorproxy进行回收,过期时间是2分钟
                // 可能由于bug或者进程挂掉，导致ActorLocationSender发送的消息没有确认，结果无法自动删除，每一分钟清理一次这种ActorLocationSender
                self.CheckTimer = TimerComponent.Instance.NewRepeatedTimer(10 * 1000, TimerInvokeType.ActorLocationSenderChecker, self);
            }
        }
        [ObjectSystem]
        public class DestroySystem: DestroySystem<ActorLocationSenderOneType> {
            protected override void Destroy(ActorLocationSenderOneType self) {
                TimerComponent.Instance?.Remove(ref self.CheckTimer);
            }
        }
        private static void Check(this ActorLocationSenderOneType self) {
            using (ListComponent<long> list = ListComponent<long>.Create()) { // 细节里：也都自带0GC 对象池缓存机制
                long timeNow = TimeHelper.ServerNow();
                foreach ((long key, Entity value) in self.Children) { // 遍历：对【实例号、 entity 对象】有序管理的字典，所有管理的子对象
                    ActorLocationSender actorLocationMessageSender = (ActorLocationSender) value;
					// 【ActorLocationSender 位置消息的发送者】超时：它们近期没活动，被视为下线掉线超时，直接移除
                    if (timeNow > actorLocationMessageSender.LastSendOrRecvTime + ActorLocationSenderOneType.TIMEOUT_TIME) {
                        list.Add(key);
                    }
                }
                foreach (long id in list) {
                    self.Remove(id); // 直接移除
                }
            }
        }
        private static ActorLocationSender GetOrCreate(this ActorLocationSenderOneType self, long id) {
            if (id == 0) { //actor 发送者实例号 0: 初生婴儿或是回收了。。
                throw new Exception($"actor id is 0");
            }
            if (self.Children.TryGetValue(id, out Entity actorLocationSender)) {
                return (ActorLocationSender) actorLocationSender;
            }
            actorLocationSender = self.AddChildWithId<ActorLocationSender>(id);
            return (ActorLocationSender) actorLocationSender;
        }
        private static void Remove(this ActorLocationSenderOneType self, long id) {
			// 对 entity 实例进程管理：
            if (!self.Children.TryGetValue(id, out Entity actorMessageSender)) {
                return;
            }
            actorMessageSender.Dispose();
        }
        // 发给不会改变位置的actorlocation用这个，这种actor消息不会阻塞发送队列，性能更高
        // 发送过去找不到actor不会重试,用此方法，你得保证actor提前注册好了location.
		// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
        public static void Send(this ActorLocationSenderOneType self, long entityId, IActorMessage message) { // 发：IActorMessage 不需要回复消息
            self.SendInner(entityId, message).Coroutine(); // 协程的调用方式
        }
        private static async ETTask SendInner(this ActorLocationSenderOneType self, long entityId, IActorMessage message) {
            ActorLocationSender actorLocationSender = self.GetOrCreate(entityId);
            if (actorLocationSender.ActorId != 0) {
                actorLocationSender.LastSendOrRecvTime = TimeHelper.ServerNow();
				// 下面没有 await 异步结果：IActorMessage 不需要回复，当然不用等什么结果
                ActorMessageSenderComponent.Instance.Send(actorLocationSender.ActorId, message); 
				// 上面【IActorLocationMessage】是当普通【IActorMessage】、不需要回复的 actor 消息，来发送出去
                return; 
            }
            long instanceId = actorLocationSender.InstanceId;
			// 对【发送者】上协程锁：先前是对【 actor 消息的、收件人邮箱】上协程锁，保障多进程安全；现在，同一个ActorLocationSender 处理发送消息，FIFO 要排队挂号
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.ActorLocationSender;
            using (await CoroutineLockComponent.Instance.Wait(coroutineLockType, entityId)) { // 【协程锁】高级封装：多进程安全下，协程锁，这里可能会等狠多桢
                if (actorLocationSender.InstanceId != instanceId) { // 上面、排队挂号、协程锁等待过程中，出错了。。比如？消息发送者不活跃超时了
                    throw new RpcException(ErrorCore.ERR_ActorTimeout, $"{message}");
                }
                 
				// 根据ActorLocationSender 它的人生、生命周期：婴儿或病逝入土，都是0. 所以要作必要的【初始化】工作
                if (actorLocationSender.ActorId == 0) { // 【初始化】工作：这个发送者，上报、注册过，它的位置吗？哪个进程的？要查位置
                    actorLocationSender.ActorId = await LocationProxyComponent.Instance.Get(self.LocationType, actorLocationSender.Id);
                    if (actorLocationSender.InstanceId != instanceId) { // 它纤进程搬家，超时了？
                        throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout2, $"{message}");
                    }
                }
                
                actorLocationSender.LastSendOrRecvTime = TimeHelper.ServerNow(); // 更新：这个位置消息发送者实例，最后活跃活动时间
				// 位置消息：最终也是带了，位置消息的发送者实例号 actorId 后，当作普通 actor 消息发送出去
                ActorMessageSenderComponent.Instance.Send(actorLocationSender.ActorId, message); 
            }
        }
        // 发给不会改变位置的actorlocation用这个，这种actor消息不会阻塞发送队列，性能更高，发送过去找不到actor不会重试
        // 发送过去找不到actor不会重试,用此方法，你得保证actor提前注册好了location
        public static async ETTask<IActorResponse> Call(this ActorLocationSenderOneType self, long entityId, IActorRequest request) { // 发：IActorRequest 需要回复消息
            ActorLocationSender actorLocationSender = self.GetOrCreate(entityId);
            if (actorLocationSender.ActorId != 0) { // 前提与确信：发给不会改变位置的actorlocation. 只要它现在有【有效合法】ActorId, 就知道它位置一定不变，直接发
                actorLocationSender.LastSendOrRecvTime = TimeHelper.ServerNow();
                return await ActorMessageSenderComponent.Instance.Call(actorLocationSender.ActorId, request); // 直接发、等跨进程位置消息发送、网络异步调用返回
            }
            long instanceId = actorLocationSender.InstanceId;
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.ActorLocationSender;
            using (await CoroutineLockComponent.Instance.Wait(coroutineLockType, entityId)) {
                if (actorLocationSender.InstanceId != instanceId) {
                    throw new RpcException(ErrorCore.ERR_ActorTimeout, $"{request}");
                }
                if (actorLocationSender.ActorId == 0) { // 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！ 
					// actorId==0应该是狠少的。多进程安全下【拿到协程锁，再等位置服返回位置消息】，协程锁里套、跨进程调用，感觉有点儿消耗，显得框架蠢笨不灵敏
					// 场景是：这个被查被发消息的实例——进程实例，进程死掉了、或是物理机宕机了。不管任何队列中排队挂号的，只要被发消息目标进程掉线了，队列里任何人都不得不等
					// 所以，也不算浪费资源、或是框架蠢笨不灵敏。就是不得不等的时候，就是安安稳稳地等，轮着谁查位置，谁拿到锁就负责去查和等！
                    actorLocationSender.ActorId = await LocationProxyComponent.Instance.Get(self.LocationType, actorLocationSender.Id);
                    if (actorLocationSender.InstanceId != instanceId) {
                        throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout2, $"{request}");
                    }
                }
            }
            actorLocationSender.LastSendOrRecvTime = TimeHelper.ServerNow();
            return await ActorMessageSenderComponent.Instance.Call(actorLocationSender.ActorId, request);
        }
		// IActorLocationMessage: 所有的位置相关消息，都是需要回复消息的
        public static void Send(this ActorLocationSenderOneType self, long entityId, IActorLocationMessage message) {
            self.Call(entityId, message).Coroutine();
        }
        public static async ETTask<IActorResponse> Call(this ActorLocationSenderOneType self, long entityId, IActorLocationRequest iActorRequest) {
            ActorLocationSender actorLocationSender = self.GetOrCreate(entityId);
            // 先序列化好【源】：【跨进程消息】这里它说的【序列化】，更像是说，把跨进程消息的信使信息 rpcId 封装好
            int rpcId = ActorMessageSenderComponent.Instance.GetRpcId();
            iActorRequest.RpcId = rpcId;
            long actorLocationSenderInstanceId = actorLocationSender.InstanceId;
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.ActorLocationSender;
            using (await CoroutineLockComponent.Instance.Wait(coroutineLockType, entityId)) { // 多进程安全下的【跨进程消息、协程锁】
				// 多进程安全下的【跨进程消息、协程锁】：执行前的、充分必要性检查：是否超时、是否异常什么的
                if (actorLocationSender.InstanceId != actorLocationSenderInstanceId) { // 排队挂号过程中，发送者不活跃、发送者超时了。。
                    throw new RpcException(ErrorCore.ERR_ActorTimeout, $"{iActorRequest}");
                }
                // 队列中没处理的消息返回跟上个消息一样的报错【源】：
				// 就是，下面的方法里有细节，当接收者失踪找不到人，不能删除，只能先给它贴个标签——此人失踪，让队列里想给它发消息的发送者知道它失踪了，可以给它发消息再重试
				// 首先，actorLocationSender.Error: 等排完队拿到协程锁、可以执行时，被通知告知，发送者所在进程异常、掉线了？总之就是找不到失踪了。。。
				// 当收件人失踪，本质就是某个发送消息者，发现揭密它失踪了，就给它贴个标签此人失踪；然后队列中，每个想要给它发消息的人、队列中没处理的消息，就都返回跟上个消息【那个最先发现它失踪的发送者】一样的报错：此人失踪！
                if (actorLocationSender.Error == ErrorCore.ERR_NotFoundActor) { 
                    return ActorHelper.CreateResponse(iActorRequest, actorLocationSender.Error);
                }
				// 拿到协程锁后，出错排查上面做完了；接下来就可以发消息了
                try {
                    return await self.CallInner(actorLocationSender, rpcId, iActorRequest);
                }
                catch (RpcException) {
                    self.Remove(actorLocationSender.Id);
                    throw;
                }
                catch (Exception e) {
                    self.Remove(actorLocationSender.Id);
                    throw new Exception($"{iActorRequest}", e);
                }
            }
        }
        private static async ETTask<IActorResponse> CallInner(this ActorLocationSenderOneType self, ActorLocationSender actorLocationSender, int rpcId, IActorRequest iActorRequest) {
            int failTimes = 0; // 重试次娄
            long instanceId = actorLocationSender.InstanceId;
            actorLocationSender.LastSendOrRecvTime = TimeHelper.ServerNow();
            while (true) { // 方法基本没用锁，包在 try..catch.. 块里
// 里面的各种【检错机制】看懂、理解透彻：【TODO】：现在感觉有些地方的检错顺序，还不懂
                if (actorLocationSender.ActorId == 0) { // 每到这种情况：就需要先【位置服】查询，接收者所在的进程位置
                    actorLocationSender.ActorId = await LocationProxyComponent.Instance.Get(self.LocationType, actorLocationSender.Id);
                    if (actorLocationSender.InstanceId != instanceId) { // 先确保这个：【TODO】：感觉这里像是没看懂
                        throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout2, $"{iActorRequest}");
                    }
                }
                if (actorLocationSender.ActorId == 0) { // 重试一遍后的：再检查，出错抛异常
                    actorLocationSender.Error = ErrorCore.ERR_NotFoundActor; // 贴个标签：此人失踪了！队列里后面排队的、也能看见这个标签 
                    return ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor); 
                }
                IActorResponse response = await ActorMessageSenderComponent.Instance.Call(actorLocationSender.ActorId, rpcId, iActorRequest, false);
                if (actorLocationSender.InstanceId != instanceId) { // 网络异步调用的结果、返回消息都收到了：为什么，这里、还再检查这个？
					// 二次确保机制：确保被查询的地址，是实时更新的！跨进程网络调用【返回消息】，还是比较耗时？这里在卡：消息回来了，但被查对象、又纤进程搬家了。。返回的消息不实时不正确
                    throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout3, $"{iActorRequest}");
                }
                switch (response.Error) {
                    case ErrorCore.ERR_NotFoundActor: {
                        // 如果没找到Actor,重试 
                        ++failTimes;
                        if (failTimes > 20)
						{ 
                            Log.Debug($"actor send message fail, actorid: {actorLocationSender.Id}");
							// 【写出错类型】：这里不能删除actor，要让后面等待发送的消息也返回ERR_NotFoundActor，直到超时删除【源】
							// 也就是，确保发送出去的消息，如果【发送者出错、但还没删除】期间、队列里的消息的发送者们，都能够得到细节通知
                            actorLocationSender.Error = ErrorCore.ERR_NotFoundActor;
                            // 这里不能删除actor，要让后面等待发送的消息也返回ERR_NotFoundActor，直到超时删除【源】
                            return response;
                        }
                        // 等待0.5s再发送
                        await TimerComponent.Instance.WaitAsync(500);
                        if (actorLocationSender.InstanceId != instanceId)
                        {
                            throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout4, $"{iActorRequest}");
                        }
                        actorLocationSender.ActorId = 0; // 四次超时后，这里就重置了
                        continue;
                    }
                    case ErrorCore.ERR_ActorTimeout: {
                        throw new RpcException(response.Error, $"{iActorRequest}");
                    }
                }
                if (ErrorCore.IsRpcNeedThrowException(response.Error)) {
                    throw new RpcException(response.Error, $"Message: {response.Message} Request: {iActorRequest}");
                }
                return response;
            }
        }
    }
    [FriendOf(typeof (ActorLocationSenderComponent))]
    public static class ActorLocationSenderManagerComponentSystem {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<ActorLocationSenderComponent> {
            protected override void Awake(ActorLocationSenderComponent self) {
                ActorLocationSenderComponent.Instance = self;
                for (int i = 0; i < self.ActorLocationSenderComponents.Length; ++i) {
                    self.ActorLocationSenderComponents[i] = self.AddChild<ActorLocationSenderOneType, int>(i);
                }
            }
        }
        [ObjectSystem]
        public class DestroySystem: DestroySystem<ActorLocationSenderComponent> {
            protected override void Destroy(ActorLocationSenderComponent self) {
                ActorLocationSenderComponent.Instance = null;
            }
        }
        public static ActorLocationSenderOneType Get(this ActorLocationSenderComponent self, int locationType) {
            return self.ActorLocationSenderComponents[locationType];
        }
    }
}