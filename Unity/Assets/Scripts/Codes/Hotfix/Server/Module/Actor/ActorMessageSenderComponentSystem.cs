using System;
using System.IO;
namespace ET.Server {
    [FriendOf(typeof(ActorMessageSenderComponent))]
    public static class ActorMessageSenderComponentSystem {
        // 要怎么理解这个消息发送超时检测中间过程步骤呢？
        // 它自带个计时器，就是说，当服务器繁忙处理不过来，它就极有可能会自动超时，若是超时了，就返回个超时消息回去发送者告知一下，必要时它可以重发。而不超时，就正常基本流程处理了
        // 那么，它就是一个服务端超负载下的自动减压逻辑
        [Invoke(TimerInvokeType.ActorMessageSenderChecker)] // 另一个新标签，激活系: 它标记说，这个激活系类，是 XXX 类型；紧跟着，就定义这个 XXX 类型的激活系类
        public class ActorMessageSenderChecker: ATimer<ActorMessageSenderComponent> {
            protected override void Run(ActorMessageSenderComponent self) { // 申明方法的接口是：ATimer<T> 抽象实现类，它实现了 AInvokeHandler<TimerCallback>
                try {
                    self.Check(); // 调用组件自己的方法
                 } catch (Exception e) {
                    Log.Error($"move timer error: {self.Id}\n{e}");
                }
            }
        }
        [ObjectSystem]
        public class ActorMessageSenderComponentAwakeSystem: AwakeSystem<ActorMessageSenderComponent> {
            protected override void Awake(ActorMessageSenderComponent self) {
                ActorMessageSenderComponent.Instance = self;
                // 组件内部重复 1000 次的计时器：
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
// Run() 方法：通过同步异常到ETTask, 通过ETTask 封装的抛异常方式抛出两类异常并返回；和对正常非异常返回消息，同步结果到ETTask, ETTask() 用触发调用注册过的非空回调
// 传进来的参数：是一个IActorResponse 实例，是有最小预处理（初始化了最基本成员变量：异常类型）、【写了个半好】的结果（异常）。结果还没同步到异步任务，待写；返回消息，待发送
        private static void Run(ActorMessageSender self, IActorResponse response) { 
            // 对于每个超时了的消息：超时错误码都是：ErrorCore.ERR_ActorTimeout, 所以会从发送消息超时异常里抛出异常，不用发送错误码【消息】回去，是抛异常
            if (response.Error == ErrorCore.ERR_ActorTimeout) { // 写：发送消息超时异常。因为同步到异步任务 ETTask 里，所以异步任务模块 ETTask会自动抛出异常
                self.Tcs.SetException(new Exception($"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: actorId: {self.ActorId} {self.Request}, response: {response}"));
                return;
            }
// 这个Run() 方法，并不是只有 Check() 【发送消息超时异常】一个方法调用。什么情况下的调用，会走到下面的分支？文件尾，有正常消息同步结果到ETTask 的调用 
// ActorMessageSenderComponent 一个组件，一次只执行一个（返回）消息发送任务，成员变量永远只管当前任务，
// 也是因为Actor 机制是并行的，一个使者一次只能发一个消息 ...
// 【组件管理器的执行频率， Run() 方法的调用频率】：要是消息太多，发不完怎么办呢？去搜索下面调用 Run() 方法的正常结果消息的调用处理频率。。。
            if (self.NeedException && ErrorCore.IsRpcNeedThrowException(response.Error)) { // 若是有异常（判断条件：消息要抛异常否？是否真有异常？），就先抛异常
                self.Tcs.SetException(new Exception($"Rpc error: actorId: {self.ActorId} request: {self.Request}, response: {response}"));
                return;
            }
            self.Tcs.SetResult(response); // 【写结果】：将【写了个半好】的消息，写进同步到异步任务的结果里；把异步任务的状态设置为完成；并触发必要的非空回调到发送者
            // 上面【异步任务 ETTask.SetResult()】，会调用注册过的一个回调，所以ETTask 封装，设置结果这一步，会自动触发调用注册过的一个回调（如果没有设置回调，因为空，就不会调用）
            // ETTask.SetResult() 异步任务写结果了，非空回调是会调用。非空回调是什么，是把返回消息发回去吗？不是。因为有独立的发送逻辑。
            // 再去想 IMHandler: 它是消息处理器。问题就变成是，当返回消息写好了，写好了一个完整的可以发送、待发送的消息，谁来处理的？有某个更底层的封装会调用这个类的发送逻辑。去把这个更底层的封装找出来，就是框架封装里，调用这个生成类Send() 方法的地方。
            // 这个服，这个自带计时器减压装配装置自带的消息处理器逻辑会处理？不是这个。减压装置，有发送消息超时，只触发最小检测，并抛发送消息超时异常给发送者告知，不写任何结果消息 
        }
        private static void Check(this ActorMessageSenderComponent self) {
            long timeNow = TimeHelper.ServerNow();
            foreach ((int key, ActorMessageSender value) in self.requestCallback) {
                // 因为是顺序发送的，所以，检测到第一个不超时的就退出
                // 超时触发的激活逻辑：是有至少一个超时的消息，才会【激活触发检测】；而检测到第一个不超时的，就退出下面的循环。
                if (timeNow < value.CreateTime + ActorMessageSenderComponent.TIMEOUT_TIME) 
                    break;
                self.TimeoutActorMessageSenders.Add(key);
            }
// 超时触发的激活逻辑：是有至少一个超时的消息，才会【激活触发检测】；而检测到第一个不超时的，就退出上面的循环。
// 检测到第一个不超时的，理论上说，一旦有一个超时消息就会触发超时检测，但实际使用上，可能存在当检测逻辑被触发走到这里，实际中存在两个或是再多一点儿的超时消息？
            foreach (int rpcId in self.TimeoutActorMessageSenders) { // 一一遍历【超时了的消息】 :
                ActorMessageSender actorMessageSender = self.requestCallback[rpcId];
                self.requestCallback.Remove(rpcId);
                try { // ActorHelper.CreateResponse() 框架系统性的封装：也是通过对消息的发送类型与对应的回复类型的管理，使用帮助类，自动根据类型统一创建回复消息的实例
                    // 对于每个超时了的消息：超时错误码都是：ErrorCore.ERR_ActorTimeout. 也就是，是个异常消息的回复消息实例生成帮助类
                    IActorResponse response = ActorHelper.CreateResponse(actorMessageSender.Request, ErrorCore.ERR_ActorTimeout);
                    Run(actorMessageSender, response); // 猜测：方法逻辑是，把回复消息发送给对应的接收消息的 rpcId
                }
                catch (Exception e) {
                    Log.Error(e.ToString());
                }
            }
            self.TimeoutActorMessageSenders.Clear();
        }
        // public static class ActorHelper {// 接上面 ActorHelper.CreateResponse() 用作参考
        //     public static IActorResponse CreateResponse(IActorRequest iActorRequest, int error) {
        //         Type responseType = OpcodeTypeComponent.Instance.GetResponseType(iActorRequest.GetType()); // 框架系统管理里，去拿返回消息的类型
        //         IActorResponse response = (IActorResponse)Activator.CreateInstance(responseType); // 创建一个返回消息的实例 instance 
        //         response.Error = error; // 写实例的出错结果、类型
        //         response.RpcId = iActorRequest.RpcId; // 返回消息的接收者 RpcId: 实际就是，消息是谁发来的，就返回消息给谁呀
        //         return response; // 返回这个最小初始化过的特定类型消息实例
        //     }
        // }
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
// 【组件管理器的执行频率， Run() 方法的调用频率】：要是消息太多，发不完怎么办呢？去搜索下面调用 Run() 方法的正常结果消息的调用处理频率。。。
// 【ActorHandleHelper 帮助类】：老是调用这里的方法，要去查那个文件。【本质：内网消息处理器的处理逻辑，一旦是返回消息，就会调用 ActorHandleHelper, 会调用这个方法来处理返回消息】        
// 下面方法：处理IActorResponse 消息，也就是，发回复消息给收消息的人XX, 那么谁发，怎么发，就是这个方法的定义
        public static void HandleIActorResponse(this ActorMessageSenderComponent self, IActorResponse response) {
            ActorMessageSender actorMessageSender;
// 下面取、实例化 ActorMessageSender 来看，感觉收消息的 rpcId, 与消息发送者 ActorMessageSender 成一一对应关系。上面的Call() 方法里，创建实例化消息发送者就是这么创始垢 
            if (!self.requestCallback.TryGetValue(response.RpcId, out actorMessageSender)) { // 这里取不到，是说，这个返回消息的发送已经被处理了？
                return;
            }
            self.requestCallback.Remove(response.RpcId); // 这个有序字典，就成为实时更新：随时添加，随时删除
            Run(actorMessageSender, response); // <<<<<<<<<<<<<<<<<<<< 
        }
    }
}


