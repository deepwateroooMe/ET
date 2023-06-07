using System;
using System.IO;
namespace ET.Server {
    [FriendOf(typeof(ActorMessageSenderComponent))]
    public static class ActorMessageSenderComponentSystem {
        [Invoke(TimerInvokeType.ActorMessageSenderChecker)] // 另一个新标签，激活系
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
// 下面的方法是：帮助发送回复消息？好像不对，感觉异步任务结果已经出来了，只差最后写结果的步骤。结果是什么时候执行的？
        private static void Run(ActorMessageSender self, IActorResponse response) { // 传进来的参数：是一个IActorResponse 实例 
            if (response.Error == ErrorCore.ERR_ActorTimeout) {
                self.Tcs.SetException(new Exception($"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: actorId: {self.ActorId} {self.Request}, response: {response}"));
                return;
            }
            // ActorMessageSenderComponent 一个组件，一次只执行一个（返回）消息发送任务，成员变量永远只管当前任务，也是因为Actor 机制是并行的，一个使者一次只能发一个消息 ...
            if (self.NeedException && ErrorCore.IsRpcNeedThrowException(response.Error)) { // 若是有异常，就先抛异常
                self.Tcs.SetException(new Exception($"Rpc error: actorId: {self.ActorId} request: {self.Request}, response: {response}"));
                return;
            }
            self.Tcs.SetResult(response); // 写结果：把异步任务的状态设置为完成，并触发必要的非空回调订阅者
        }
        private static void Check(this ActorMessageSenderComponent self) {
            long timeNow = TimeHelper.ServerNow();
            foreach ((int key, ActorMessageSender value) in self.requestCallback) {
                // 因为是顺序发送的，所以，检测到第一个不超时的就退出
                if (timeNow < value.CreateTime + ActorMessageSenderComponent.TIMEOUT_TIME) 
                    break;
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
        public static void Send(this ActorMessageSenderComponent self, long actorId, IMessage message) { // 发消息 
            if (actorId == 0) {
                throw new Exception($"actor id is 0: {message}");
            }
            ProcessActorId processActorId = new(actorId);
            // 这里做了优化，如果发向同一个进程，则直接处理，不需要通过网络层
            if (processActorId.Process == Options.Instance.Process) { // 没看懂：这里怎么就说，消息是发向同一进程的了？
                NetInnerComponent.Instance.HandleMessage(actorId, message);
                return;
            }
            Session session = NetInnerComponent.Instance.Get(processActorId.Process);
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
        public static async ETTask<IActorResponse> Call( // 发消息：细节比较难懂。感觉还是对ETTask 异步任务没能理解透彻
            this ActorMessageSenderComponent self,
            long actorId,
            int rpcId,
            IActorRequest iActorRequest,
            bool needException = true
            ) {
            if (actorId == 0) {
                throw new Exception($"actor id is 0: {iActorRequest}");
            }
            var tcs = ETTask<IActorResponse>.Create(true); // 对象池里：取一个异步任务。用这个异步作务实例，去创建下面的消息发送者实例
            self.requestCallback.Add(rpcId, new ActorMessageSender(actorId, iActorRequest, tcs, needException)); // 对照发返回消息，再看一遍
            self.Send(actorId, iActorRequest); // 把请求消息发出去
            long beginTime = TimeHelper.ServerFrameTime();
// 【难点】：两个类，当前类，与ETTask，感觉每个词都看懂了，下面一行，连一起，就不明白，它在干什么？
            // 自己想一下的话：异步消息发出去，某个服会处理，有返回消息的话，这个服处理后会返回一个返回消息。
            // 那么下面一行，不是等待创建 Create() 异步任务，是等待这个处理发送消息的服，返回来返回消息，或说把返回消息的内容填好，应该还没发回到消息发送者 
            IActorResponse response = await tcs; // 【稀里糊涂，有点儿不懂】：等异步任务的创建完成，实际是等处理发送消息的服，处理完并写好返回消息
            long endTime = TimeHelper.ServerFrameTime();
            long costTime = endTime - beginTime;
            if (costTime > 200) {
                Log.Warning($"actor rpc time > 200: {costTime} {iActorRequest}");
            }
            return response;
        }
        // 下面方法：处理IActorResponse 消息，也就是，发回复消息给收消息的人XX, 那么谁发，怎么发，就是这个方法的定义
        public static void HandleIActorResponse(this ActorMessageSenderComponent self, IActorResponse response) {
            ActorMessageSender actorMessageSender;
// 下面取、实例化 ActorMessageSender 来看，感觉收消息的 rpcId, 与消息发送者 ActorMessageSender 成一一对应关系。上面的Call() 方法里，创建实例化消息发送者就是这么创始垢 
            if (!self.requestCallback.TryGetValue(response.RpcId, out actorMessageSender)) { // 这里取不到，是说，这个返回消息的发送已经被处理了？
                return;
            }
            self.requestCallback.Remove(response.RpcId); // 这个有序字典，就成为实时更新：随时添加，随时删除
            Run(actorMessageSender, response);
        }
    }
}