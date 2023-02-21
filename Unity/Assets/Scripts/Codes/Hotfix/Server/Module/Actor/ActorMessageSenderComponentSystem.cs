using System;
using System.IO;
namespace ET.Server {
    [FriendOf(typeof(ActorMessageSenderComponent))]
    public static class ActorMessageSenderComponentSystem {
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
                ActorMessageSender actorMessageSender = self.requestCallback[rpcId];
                self.requestCallback.Remove(rpcId);
                try {
                    IActorResponse response = ActorHelper.CreateResponse(actorMessageSender.Request, ErrorCore.ERR_ActorTimeout);
                    Run(actorMessageSender, response);
                }
                catch (Exception e) {
                    Log.Error(e.ToString());
                }
            }
            self.TimeoutActorMessageSenders.Clear();
        }
        // 调用actorMessageSender的CallWithoutException方法，内部封装了NetInnerComponent以actorMessageSender内部存放的地址获取一个Session，然后调用Session的CallWithoutException即消息发送出去。 
        public static void Send(this ActorMessageSenderComponent self, long actorId, IMessage message) {
            if (actorId == 0) {
                throw new Exception($"actor id is 0: {message}");
            }
            ProcessActorId processActorId = new(actorId);
            // 这里做了优化，如果发向同一个进程，则直接处理，不需要通过网络层
            if (processActorId.Process == Options.Instance.Process) { // Options.Instance.Process: 当前服务器进程
                NetInnerComponent.Instance.HandleMessage(actorId, message); // 直接触发回调：相关注册过对这类消息有兴趣的，都会回调一次
                // 上面 NetInnerComponent： 这里有一点儿昏昏的，不是说内网是服务器之间的消息传递，外网是客户端与服务器之间吗？还是说，我把进程没理解对？
                return;
            }
            Session session = NetInnerComponent.Instance.Get(processActorId.Process); // 通过会话框，将消息发出去
            session.Send(processActorId.ActorId, message);
        }
        public static int GetRpcId(this ActorMessageSenderComponent self) {
            return ++self.RpcId;
        }
        public static async ETTask<IActorResponse> Call(
            this ActorMessageSenderComponent self,
            long actorId,
            IActorRequest request,
            bool needException = true
            ) {
            request.RpcId = self.GetRpcId();
            
            if (actorId == 0) {
                throw new Exception($"actor id is 0: {request}");
            }
            return await self.Call(actorId, request.RpcId, request, needException);
        }
        // 调用actorMessageSender的CallWithoutException方法，内部封装了NetInnerComponent以actorMessageSender内部存放的地址获取一个Session，然后调用Session的CallWithoutException即消息发送出去。
        public static async ETTask<IActorResponse> Call(this ActorMessageSenderComponent self, long actorId, int rpcId, IActorRequest iActorRequest, bool needException = true) {
            if (actorId == 0) {
                throw new Exception($"actor id is 0: {iActorRequest}");
            }
            var tcs = ETTask<IActorResponse>.Create(true);
            self.requestCallback.Add(rpcId, new ActorMessageSender(actorId, iActorRequest, tcs, needException)); // 升序字典：就是方便查找 ?
            self.Send(actorId, iActorRequest);
            long beginTime = TimeHelper.ServerFrameTime();
            IActorResponse response = await tcs;
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