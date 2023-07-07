using System;
using System.IO;
using MongoDB.Bson;
namespace ET.Server {

    [Invoke(TimerInvokeType.ActorLocationSenderChecker)]
    public class ActorLocationSenderChecker: ATimer<ActorLocationSenderComponent> {
        protected override void Run(ActorLocationSenderComponent self) {
            try {
                self.Check();
            } catch (Exception e) {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }
    [ObjectSystem]
    public class ActorLocationSenderComponentAwakeSystem: AwakeSystem<ActorLocationSenderComponent> {
        protected override void Awake(ActorLocationSenderComponent self) {
            ActorLocationSenderComponent.Instance = self;
            // 每10s扫描一次过期的actorproxy进行回收,过期时间是2分钟
            // 可能由于bug或者进程挂掉，导致ActorLocationSender发送的消息没有确认，结果无法自动删除，每一分钟清理一次这种ActorLocationSender
            self.CheckTimer = TimerComponent.Instance.NewRepeatedTimer(10 * 1000, TimerInvokeType.ActorLocationSenderChecker, self);
        }
    }
    [ObjectSystem]
    public class ActorLocationSenderComponentDestroySystem: DestroySystem<ActorLocationSenderComponent> {
        protected override void Destroy(ActorLocationSenderComponent self) {
            ActorLocationSenderComponent.Instance = null;
            TimerComponent.Instance?.Remove(ref self.CheckTimer);
        }
    }

    [FriendOf(typeof(ActorLocationSenderComponent))]
    [FriendOf(typeof(ActorLocationSender))]
    public static class ActorLocationSenderComponentSystem {
        public static void Check(this ActorLocationSenderComponent self) {
            using (ListComponent<long> list = ListComponent<long>.Create()) { // Using: 作用是，逻辑执行完毕，会可以自动回收
                long timeNow = TimeHelper.ServerNow();
                foreach ((long key, Entity value) in self.Children) {
                    ActorLocationSender actorLocationMessageSender = (ActorLocationSender) value;
                    if (timeNow > actorLocationMessageSender.LastSendOrRecvTime + ActorLocationSenderComponent.TIMEOUT_TIME) 
                        list.Add(key);
                }
                // 如果要拿位置的被请求对象， 60 秒里不活动（目标对象，60 秒内不曾发送消息或是接收消息），这里是【位置服】对所管理对象的自动消号处理
                // 当前【全局单例位置管理器】：视其为掉线或是下线，自动移除其位置消息，视为不曾见过不知道
                foreach (long id in list) {
                    self.Remove(id);
                }
            }
        }
        private static ActorLocationSender GetOrCreate(this ActorLocationSenderComponent self, long id) { // 拿，或者创建新的
            if (id == 0) 
                throw new Exception($"actor id is 0");
            if (self.Children.TryGetValue(id, out Entity actorLocationSender)) { // 有就直接返回
                return (ActorLocationSender) actorLocationSender;
            } // 下面，没有就创建一个新的
            actorLocationSender = self.AddChildWithId<ActorLocationSender>(id); // 当子控件来添加的，不是加入字典里。回收时就需要回收子控件
            return (ActorLocationSender) actorLocationSender;
        }
        private static void Remove(this ActorLocationSenderComponent self, long id) {
            if (!self.Children.TryGetValue(id, out Entity actorMessageSender))  // 字典里没有，就自动返回，不用管了
                return;
            // 这里不是字典，是【遍历真正的子控件】。字典需要清除键值对，子控件，只需要子控件回收
            actorMessageSender.Dispose();
        }
        // 【发送：请求位置的消息】
        public static void Send(this ActorLocationSenderComponent self, long entityId, IActorRequest message) {
            self.Call(entityId, message).Coroutine(); // 调用异步方法
        }
        public static async ETTask<IActorResponse> Call(this ActorLocationSenderComponent self, long entityId, IActorRequest iActorRequest) {
            ActorLocationSender actorLocationSender = self.GetOrCreate(entityId);
            // 【先序列化好】：前面原标注。这里序列化，是指把索要位置消息的发送者与接收者等相关必要信息，这个位置管理器组件，管理好，给每个弄个身份证就可以区分了
            int rpcId = ActorMessageSenderComponent.Instance.GetRpcId(); // 为什么要跑去找ActorMessageSenderComponent 来拿 rpcId ？
            iActorRequest.RpcId = rpcId;
            long actorLocationSenderInstanceId = actorLocationSender.InstanceId; // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！】
            // 上午看到这里：跑去看锁了，结果看得半懂不懂的，明天上午再好好看一下锁的部分。
            // 【协程锁组件】：等待创建协程锁异步方法，并会等 1 分钟：（这里默认是得等60 秒。活宝妹已经坐了一年冷板凳，活宝妹为了要为了能够嫁给活宝妹的亲爱的表哥，活宝妹会需要再坐一年冷板凳？）
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.ActorLocationSender, entityId)) {
                if (actorLocationSender.InstanceId != actorLocationSenderInstanceId) // 上面没明白：为什么要等 60 秒。这里1 分钟前后的实例 id 不一样，说明出错出异常了 
                    throw new RpcException(ErrorCore.ERR_ActorTimeout, $"{iActorRequest}");
                // 队列中没处理的消息返回跟上个消息一样的报错
                if (actorLocationSender.Error == ErrorCore.ERR_NotFoundActor) // 返回出错：调用框架包装的一个简单泛用出错包装类，把出错结果写好
                    return ActorHelper.CreateResponse(iActorRequest, actorLocationSender.Error);
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
        private static async ETTask<IActorResponse> CallInner(this ActorLocationSenderComponent self, ActorLocationSender actorLocationSender, int rpcId, IActorRequest iActorRequest) {
            int failTimes = 0;
            long instanceId = actorLocationSender.InstanceId;
            actorLocationSender.LastSendOrRecvTime = TimeHelper.ServerNow();
            while (true) { // 无限循环
                if (actorLocationSender.ActorId == 0) { // 试着重新拿了一次
                    actorLocationSender.ActorId = await LocationProxyComponent.Instance.Get(actorLocationSender.Id);
                    if (actorLocationSender.InstanceId != instanceId) 
                        throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout2, $"{iActorRequest}");
                }
                if (actorLocationSender.ActorId == 0) { // 仍为 0, 写出错结果
                    actorLocationSender.Error = ErrorCore.ERR_NotFoundActor;
                    return ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                }
                // 发送索要位置信息：要求不抛异常。那么就是位置消息发送组件自动移除了超时消息，不反馈给发送端。发送端收不到返回，自已决定要不要再重发索要位置的消息 
                IActorResponse response = await ActorMessageSenderComponent.Instance.Call(actorLocationSender.ActorId, rpcId, iActorRequest, false);
                if (actorLocationSender.InstanceId != instanceId) {
                    throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout3, $"{iActorRequest}");
                }
                switch (response.Error) {
                    case ErrorCore.ERR_NotFoundActor: {
                        // 如果没找到Actor,重试
                        ++failTimes;
                        if (failTimes > 20) {
                            Log.Debug($"actor send message fail, actorid: {actorLocationSender.Id}");
                            actorLocationSender.Error = ErrorCore.ERR_NotFoundActor;
                            // 这里不能删除actor，要让后面等待发送的消息也返回ERR_NotFoundActor，直到超时删除
                            return response;
                        }
                        // 等待0.5s再发送
                        await TimerComponent.Instance.WaitAsync(500);
                        if (actorLocationSender.InstanceId != instanceId)
                            throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout4, $"{iActorRequest}");
                        actorLocationSender.ActorId = 0; // 这里先重置为0, 上面有逻辑会重新再设置一次
                        continue;
                    }
                case ErrorCore.ERR_ActorTimeout: // 发送索要位置信息，要求不抛异常。这里自已断定消息超时，自己手动抛异常 
                        throw new RpcException(response.Error, $"{iActorRequest}");
                }
                if (ErrorCore.IsRpcNeedThrowException(response.Error)) {
                    throw new RpcException(response.Error, $"Message: {response.Message} Request: {iActorRequest}");
                }
                return response; // 是返回的正常位置消息，就返回
            }
        }
    }
}