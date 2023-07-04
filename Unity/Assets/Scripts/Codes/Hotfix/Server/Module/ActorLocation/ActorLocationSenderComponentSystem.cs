using System;
using System.IO;
using MongoDB.Bson;
namespace ET.Server {
    [Invoke(TimerInvokeType.ActorLocationSenderChecker)]
    public class ActorLocationSenderChecker: ATimer<ActorLocationSenderComponent> {
        protected override void Run(ActorLocationSenderComponent self) {
            try {
                self.Check();
            }
            catch (Exception e) {
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
            using (ListComponent<long> list = ListComponent<long>.Create()) {
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
            actorLocationSender = self.AddChildWithId<ActorLocationSender>(id); // 当子控件
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
            long actorLocationSenderInstanceId = actorLocationSender.InstanceId;
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.ActorLocationSender, entityId)) {
                if (actorLocationSender.InstanceId != actorLocationSenderInstanceId) 
                    throw new RpcException(ErrorCore.ERR_ActorTimeout, $"{iActorRequest}");
                // 队列中没处理的消息返回跟上个消息一样的报错
                if (actorLocationSender.Error == ErrorCore.ERR_NotFoundActor) 
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
            while (true) {
                if (actorLocationSender.ActorId == 0) {
                    actorLocationSender.ActorId = await LocationProxyComponent.Instance.Get(actorLocationSender.Id);
                    if (actorLocationSender.InstanceId != instanceId) 
                        throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout2, $"{iActorRequest}");
                }
                if (actorLocationSender.ActorId == 0) {
                    actorLocationSender.Error = ErrorCore.ERR_NotFoundActor;
                    return ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor);
                }
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
                        actorLocationSender.ActorId = 0;
                        continue;
                    }
                    case ErrorCore.ERR_ActorTimeout: 
                        throw new RpcException(response.Error, $"{iActorRequest}");
                }
                if (ErrorCore.IsRpcNeedThrowException(response.Error)) {
                    throw new RpcException(response.Error, $"Message: {response.Message} Request: {iActorRequest}");
                }
                return response;
            }
        }
    }
}