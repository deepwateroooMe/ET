using System;
using System.IO;
using MongoDB.Bson;
namespace ET.Server {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	// 这个模块，现在看懂了！【超时机制】：也总算基本、都、看懂了！！

[FriendOf(typeof(ActorLocationSenderOneType))]
    [FriendOf(typeof(ActorLocationSender))]
    public static class ActorLocationSenderComponentSystem {
        [Invoke(TimerInvokeType.ActorLocationSenderChecker)] // 下面的重复闹钟：闹的时候，就会调用下面的回调: 周期闹钟、回调就会周期性地检测超时的位置消息发送实例
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
				// 【过期时间现在应该是 1 分钟；每 10 秒钟，是重复闹钟的周期检测，回收过期代理；也是文件后面的发送重试最多试10 秒钟】
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
// 标签申明过：它是 ActorLocationSenderOneType 类型的【子控件】所以会，添加子Component 父子关系
            actorLocationSender = self.AddChildWithId<ActorLocationSender>(id); // 新创建的，这里其已经 InstanceId ！＝ 0 了
			// 当它【从对象池】或是新生成一个ActorLocationSender 的实例，entity 的AddChildWithId() 方法定义里，建立父子关系后，会调用ActorLocationSender 的Awake()生命周期，Awake() 时其 ActorId ＝ 0 被赋值0
            return (ActorLocationSender) actorLocationSender; 
			// 这些，也是理解框架里，【位置服】对跨进程消息传递信使的、生命周期管理，必需的
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
		// 现在，感觉这个方法完全看懂了【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
        public static void Send(this ActorLocationSenderOneType self, long entityId, IActorMessage message) { // IActorMessage 不需要回复消息
            self.SendInner(entityId, message).Coroutine(); // 协程的调用方式
        }
        private static async ETTask SendInner(this ActorLocationSenderOneType self, long entityId, IActorMessage message) {
			// entityId 本身代一个 entity 实例的已经存在【可以是玩家、其登录逻辑里会注册上报位置服其实例号】；这里只是另一层面的包装
            ActorLocationSender actorLocationSender = self.GetOrCreate(entityId); // 拿现的、生成新的。下面也分情况分支
            if (actorLocationSender.ActorId != 0) { // 有位置信息的，应该是向【位置服】注册过、被管理的 
                actorLocationSender.LastSendOrRecvTime = TimeHelper.ServerNow();
				// 下面没有 await 异步结果：IActorMessage 不需要回复，当然不用等什么结果
                ActorMessageSenderComponent.Instance.Send(actorLocationSender.ActorId, message); // 直接发消息出去就可以了 
				// 上面【IActorLocationMessage】是当普通【IActorMessage】、不需要回复的 actor 消息，来发送出去
                return; 
            }
			// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
			// 如果是【新创建的actorLocationSender 实例】，这里就一定会被锁；因为现在想要给它发消息的 entity, 并不知道它的实例号，无法发需要查询位置服方可知道；被锁时，所有想向它发消息的，全放入队列等待
            long instanceId = actorLocationSender.InstanceId; // instanceId 就是不为 0, 现在基本认定
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.ActorLocationSender;
// 【协程锁】高级封装：多进程安全下协程锁，锁跨进程消息的收件人，多个进程实例，都想给它发消息，它队伍FIFO处理
            using (await CoroutineLockComponent.Instance.Wait(coroutineLockType, entityId)) { 
// 上面、排队挂号、协程锁等待过程中，各种可能的原因，实例号变了【最大可能是超时回收，消值为0 了；纤进程的可能性也有，就要查询后再重发】
                if (actorLocationSender.InstanceId != instanceId) { // 最大可能性：现在的 actorLocationSender 因为超时被回收，其InstanceId = 0; 也可能是纤进程了
                    throw new RpcException(ErrorCore.ERR_ActorTimeout, $"{message}"); //entity 下线登出了或纤进程了，前进程实例回收了，超时1 
                }
                // 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
				// 【再检查】一遍ActorId == 0：等待协程锁的过程中，它可能不是队列里第一个、务必去查询位置服这个 entity.Id 实例号的发送者；队列前面的人，可能已经帮它解决了
                if (actorLocationSender.ActorId == 0) { // 向【位置服】查询、它的实例号。因为它前段时间不活跃，被置0 释放系统资源了
					// 向【位置服】Get() 拿的是，对应这个Entity.Id 的 InstanceId 【相当于，注册上报位置服的实例号、或是纤进程后的新进程里实例号】
					// 发送者新实例，什么时候，曾经向【位置服】注册上报过实例号？查询位置服，仅只是去查、去拿，去读实例号，什么时候注册、上报、写过、保存过实例号？
					// entityId 的存在，标记：本身已有这个 entityId 的玩家存在、玩家的 InstanceId 也存在。只是，发送者不知道玩家 entityId 所用的现实例号 InstanceId, 所以要去查询位置服
                    actorLocationSender.ActorId = await LocationProxyComponent.Instance.Get(self.LocationType, actorLocationSender.Id); // 拿最新进程位置，好像写得不对！！
					// 【跨进程网络异步调用】还是狠耗时的；并不知道【位置服】的处理压力，查询一次位置往返需要耗时多久；所以位置服现返回的位置，并不一定是现实例的精确、正确现状态
                    if (actorLocationSender.InstanceId != instanceId) { // InstanceId是可以变化的；如果它变化了，就是收件人登出下线销毁、或是纤进程搬家了
                        throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout2, $"{message}"); // 超时2: 查询位置服的异步网络调用过程中，再次超时。。
                    }
                }
				// 精确知道：现接收者、在线和实例号，可以发消息
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
// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
                if (actorLocationSender.ActorId == 0) { // 新实例化的 actorLocationSender, 需要去查它的实例号
                    actorLocationSender.ActorId = await LocationProxyComponent.Instance.Get(self.LocationType, actorLocationSender.Id);
                    if (actorLocationSender.InstanceId != instanceId) { // 查询位置服网络异步调用过程中，发送者超时
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
		// 自己：发给位置可能会频繁改变的，用这个。需要确保位置正确，必要时去查位置信息，出错后会重试
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
                // 队列中没处理的消息返回跟上个消息一样的报错【源】：先查标签、最快最高效！发送者被贴找不到，就不用浪费太多时间，直接返回错误
                if (actorLocationSender.Error == ErrorCore.ERR_NotFoundActor) { 
                    return ActorHelper.CreateResponse(iActorRequest, actorLocationSender.Error);
                }
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
            int failTimes = 0; // 重试次数
            long instanceId = actorLocationSender.InstanceId;
            actorLocationSender.LastSendOrRecvTime = TimeHelper.ServerNow();
            while (true) { // 方法基本没用锁，包在 try..catch.. 块里
                if (actorLocationSender.ActorId == 0) { // 被重置为0：曾经不活跃、迁进程等等过；就需要先【位置服】查询，接收者所在的实例号
                    actorLocationSender.ActorId = await LocationProxyComponent.Instance.Get(self.LocationType, actorLocationSender.Id); 
                    if (actorLocationSender.InstanceId != instanceId) { // 【查询位置服、异步网络调用、往返消息】比较耗时；这1 个查询位置服调用过程中，发送者超时
                        throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout2, $"{iActorRequest}");
                    }
                }
                if (actorLocationSender.ActorId == 0) { // 【位置服查询结果】说，发送者没注册过、登出下线了。。【因为只要登录，就会有有效非0 实例号】
                    actorLocationSender.Error = ErrorCore.ERR_NotFoundActor; // 贴个标签：此人登出下线失踪了！队列里后面排队的、也能看见这个标签，会同样处理返回给发消息方 
                    return ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor); 
                }
                IActorResponse response = await ActorMessageSenderComponent.Instance.Call(actorLocationSender.ActorId, rpcId, iActorRequest, false);
// 【卡得极严密】：不管返回【正常、或异常、结果 response】，先O(1) 时间确保，结果“绝对”正确！
// 但这个绝对仍然是相对的，因为返回正确结果后才超时或登出，这里仍然存在【假阳性超时】，就是，玩家下线前收到了且返回了消息，后才下线，但这里会误作它没收到它超时了。。视返回结果为不看。。
				// 这些，应该是设计层面的、出于使用情境、各种使用用例的考量，来决定如何设计与逻辑。。。
                if (actorLocationSender.InstanceId != instanceId) { 
// 二次确保机制：确保被查询的地址，是实时更新的！跨进程网络调用【返回消息】，还是比较耗时？这里在卡：消息回来了，但被查对象、又纤进程搬家了。。返回的消息不实时不正确
					// 那么上面【假阳性】：也就是，出于是否，要向发送者，连续接着继续发消息的情况。这里异常了，就不会浪费再后面的发送、抛错、出错、资源
                    throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout3, $"{iActorRequest}");
                } 
                switch (response.Error) { // 现在看这个排错检测，看得狠懂了。【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
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
                        { // 任何时候，收件人因纤进程而改变了实例号 instanceId, 或是超时被回收，都抛异常。。
                            throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout4, $"{iActorRequest}");
                        }
                        actorLocationSender.ActorId = 0;
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