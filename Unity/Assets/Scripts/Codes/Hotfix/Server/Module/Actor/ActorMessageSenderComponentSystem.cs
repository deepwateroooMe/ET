using System;
using System.IO;
namespace ET.Server {

    [FriendOf(typeof(ActorMessageSenderComponent))]
    public static class ActorMessageSenderComponentSystem {
        // 它自带个计时器，就是说，当服务器繁忙处理不过来，它就极有可能会自动超时，若是超时了，就抛个超时异常回去告知发送者一下，必要时它可以重发。
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
                // 组件内部每秒钟重复一次的闹钟：跟贱畜牲发疯犯贱一样频繁，把人类恶心坏了。。
                // 这个重复闹钟，是消息自动计时超时过滤器的上下文连接桥梁
                // 它注册的回调 TimerInvokeType.ActorMessageSenderChecker, 会每个消息超时的时候，都会回来调用 checker 的 Run()==>Check() 方法
                // 应该是重复闹钟每秒重复一次，就每秒检查一次，调用一次Check() 方法来检查超时？是过滤器会给服务器减压；但这里的自动检测会把压分在各消息发送组件服务器上
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

// Run() 方法：通过同步异常到ETTask, 通过ETTask 封装的抛异常方式抛出两类异常并返回；和对正常非异常【返回消息】，同步结果到ETTask
// 传进来的参数：是一个IActorResponse 实例，是有最小预处理（初始化了最基本成员变量：异常类型）、【写了个半好】的结果（异常）。结果还没同步到异步任务，待写
        // 【返回消息】的返回过程：是在下面的Call() 方法【发送消息的过程】的调用逻辑里，直接返回异步任务的结果，异步给调用方
        private static void Run(ActorMessageSender self, IActorResponse response) { // 写，同步【封装的异步任务Tcs】的异常或正常结果，只写结果 
            // 对于每个超时了的消息：超时错误码都是：ErrorCore.ERR_ActorTimeout, 所以会从异步任务模块里抛出异常，不用发送错误码【消息】回去，是抛异常
            if (response.Error == ErrorCore.ERR_ActorTimeout) { // 写：发送消息超时异常。因为同步到异步任务 ETTask 里，所以异步任务模块 ETTask会自动抛出异常
                self.Tcs.SetException(new Exception($"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: actorId: {self.ActorId} {self.Request}, response: {response}"));
                return;
            }
// 这个Run() 方法，并不是只有 Check() 【发送消息超时异常】一个方法调用。什么情况下的调用，会走到下面的分支？文件尾，有正常【返回消息，写框架组件ActorMessageAender 封装的异步任务 Tcs】同步结果到ETTask 的调用 
// ActorMessageSenderComponent 一个组件，一次只执行一个（返回？不明白自己当初这里写的是什么意思）消息发送任务，成员变量永远只管当前任务，
// 也是因为Actor 机制是并发的，一个使者一次只能发一个消息 ...? 这些是在说什么呢？现在说不懂
// 【组件管理器的执行频率， Run() 方法的调用频率】：Run() 这个方法，是写【返回消息】结果到异步任务，用于桥接返回给调用方。每有一个返回Actor消息，都会执行一次这个写结果的方法
            if (self.NeedException && ErrorCore.IsRpcNeedThrowException(response.Error)) { // 若是有异常（判断条件：消息要抛异常否？是否真有异常？），就先抛异常
                self.Tcs.SetException(new Exception($"Rpc error: actorId: {self.ActorId} request: {self.Request}, response: {response}"));
                return;
            }
            self.Tcs.SetResult(response); // 【写结果】：将【写了个半好】的消息，写进同步到异步任务的结果里；把异步任务的状态设置为完成；
            // 发送需要【返回消息】的逻辑里，借助临时异步任务变量 tcs, 桥接给ActorMessageSender.
            // 当这里的结果写好，发送需要【返回消息】的调用逻辑【下面的Call() 方法】里，就可以把 tcs 异步结果返回给调用方。后续逻辑是在调用处桥接好了的
        }
        private static void Check(this ActorMessageSenderComponent self) { // 【单例组件】的自清理逻辑：遍历所有的发送代理，剔除超时的
            long timeNow = TimeHelper.ServerNow();
            foreach ((int key, ActorMessageSender value) in self.requestCallback) { // 有序字典
                // 因为是顺序发送的，所以，检测到第一个不超时的就退出.
                // 【顺序发送】，按发送时间（超时）从小到大排列。只要队头最小时间不超时，以后的也不会超时（同组件超时时长一致）；而若消息超时，遍历到第一个不超时的那个消息，退出循环
                if (timeNow < value.CreateTime + ActorMessageSenderComponent.TIMEOUT_TIME) 
                    break;
                self.TimeoutActorMessageSenders.Add(key);
            }
            foreach (int rpcId in self.TimeoutActorMessageSenders) { // 一一遍历【超时了的消息】 :
                ActorMessageSender actorMessageSender = self.requestCallback[rpcId];
                self.requestCallback.Remove(rpcId);
                try { // ActorHelper.CreateResponse() 框架系统性的封装：也是通过对消息的发送类型与对应的回复类型的管理，使用帮助类，自动根据类型统一创建回复消息的实例
                    // 对于每个超时了的消息：超时错误码都是：ErrorCore.ERR_ActorTimeout. 也就是，是个异常消息的回复消息实例生成帮助类
                    IActorResponse response = ActorHelper.CreateResponse(actorMessageSender.Request, ErrorCore.ERR_ActorTimeout);
                    Run(actorMessageSender, response); // 写异常结果，到Tcs 异步任务。【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
                } catch (Exception e) {
                    Log.Error(e.ToString());
                }
            }
            self.TimeoutActorMessageSenders.Clear();
        }
        // 【发送消息】：框架里所有消息的发送，都走这个方法 
        public static void Send(this ActorMessageSenderComponent self, long actorId, IMessage message) { // 发消息：这个方法，发所有类型的消息，最基接口IMessage
            if (actorId == 0) 
                throw new Exception($"actor id is 0: {message}");
            ProcessActorId processActorId = new(actorId); // 这里是聪明的实例 id 的好处：可以自带进程信息，可以通过位操作拿到。拿到的是，接收消息的 actorId 所在的进程号，向那个进程发送消息
            // 这里做了优化，如果发向同一个进程，则直接处理，不需要通过网络层。（前面源者注。这里理解为同一进程，不同SceneType 场景上的消息，不需要走网络层）
            if (processActorId.Process == Options.Instance.Process) { // 【左边】：消息发出方的进程号；【右边】：应该是当前进程号。【右边】没能看懂
                NetInnerComponent.Instance.HandleMessage(actorId, message); // 原理清楚：本进程消息，直接交由本进程内网组件处理
                return;
            }
            Session session = NetInnerComponent.Instance.Get(processActorId.Process); // 非本进程消息，去走网络层
            session.Send(processActorId.ActorId, message);
        }
        public static int GetRpcId(this ActorMessageSenderComponent self) { // 自家组件的局部身份证索引号管理：自增局部变量，维护管理体系里注册过的各种回调
            return ++self.RpcId;
        }
        // 这个方法：只对当前进程的发送要求IActorResponse 的消息，封装自家进程的 rpcId, 也就是标明本进程发的消息，来自其它进程的返回消息，到时发到本进程。是特殊使用
        // 上面，不知道自己写的是什么？好像上面写的不对，要再检查
        public static async ETTask<IActorResponse> Call(
            this ActorMessageSenderComponent self,
            long actorId,
            IActorRequest request,
            bool needException = true
            ) {
            request.RpcId = self.GetRpcId(); // 封装本进程的 rpcId 
            if (actorId == 0) throw new Exception($"actor id is 0: {request}");
            return await self.Call(actorId, request.RpcId, request, needException);
        }
        // 【艰森诲涩难懂！！】是更底层的实现细节，它封装帮助实现ET7 里消息超时自动过滤抛异常、返回消息的底层封装自动回复、封装了异步任务和必要成员变量来实现这些辅助过滤器等功能 
        public static async ETTask<IActorResponse> Call( // 跨进程发请求消息（要求回复）：返回跨进程异步调用结果。是 await 关键字调用，用在异步方法里
            this ActorMessageSenderComponent self,
            long actorId,
            int rpcId,
            IActorRequest iActorRequest,
            bool needException = true
            ) {
            if (actorId == 0) 
                throw new Exception($"actor id is 0: {iActorRequest}");
// 对象池里：取一个异步任务。用这个异步作务实例，去创建下面的消息发送器实例。这里的 IActorResponse T 应该只是一个索引。因为前面看见系统扫描标签系创建返回实例，套到这个索引
            var tcs = ETTask<IActorResponse>.Create(true); // 【逻辑的连接在这个变量】：组件（相对来说是，框架的相对底层）创建一个本地变量，用来桥接异步网络调用的结果
            // 封装好消息发送器，交由消息发送组件管理；交由其管理，就自带消息发送计时超时过滤机制，实现服务器超负荷时的自动分压减压处理。一旦超时自动报废。。。忘记了它返不返回什么超时异常？
            self.requestCallback.Add(rpcId, new ActorMessageSender(actorId, iActorRequest, tcs, needException));  // 【tcs 异步网络调用桥接变量：】
            self.Send(actorId, iActorRequest); // 把请求消息发出去：组件的更底层封装里，会把异步调用的异步返回结果，写进 tcs 里
            long beginTime = TimeHelper.ServerFrameTime();
// 自己想一下的话：异步消息发出去，某个服会处理，有返回消息的话，这个服处理后会返回一个返回消息。
// 那么下面一行，不是等待创建 Create() 异步任务（同步方法狠快），而是【等待这个处理发送消息的服，处理并返回来返回消息（是说，那个服，把处理结果写好、同步到异步任务）】【在上面 Send() 方法里】
// 不是等异步任务的创建完成（同步方法狠快），实际是【等处理发送消息的服，处理完并写好返回消息，同步到异步任务】【在上面 Send() 方法里】
// 那个ETTask 里的回调 callback，是怎么回调的？这里的Tcs 没有设置任何回调。ETTask 里所谓回调，是执行异步状态机的下一步，没有实际应用层面的回调意义
// 或说把返回消息的内容填好，【应该还没发回到消息发送者？是的】返回消息填好了，ETTask 异步任务 tcs的结果同步到位了，就可以把【返回消息】给返回回给调用方了呀
// 【异步任务结果是怎么回来的？】这个方法里，不是定义了内部临时变量 tcs, 用来桥接异步网络调用的结果吗？当ActorMessageSender 【 line 128 Send()】
            IActorResponse response = await tcs;  // 等待消息处理服处理完，写好同步好结果到异步任务、异步任务执行完成，状态为 Succeed
            long endTime = TimeHelper.ServerFrameTime();
            long costTime = endTime - beginTime;
            if (costTime > 200) 
                Log.Warning($"actor rpc time > 200: {costTime} {iActorRequest}");
            return response; // 返回：异步网络调用的结果
        }
// 【组件管理器的执行频率， Run() 方法的调用频率】：要是消息太多，发不完怎么办呢？去搜索下面调用 Run() 方法的正常结果消息的调用处理频率。。。
// 【ActorHandleHelper 帮助类】：老是调用这里的方法，要去查那个文件。【本质：内网消息处理器的处理逻辑，一旦是返回消息，就会调用 ActorHandleHelper, 会调用这个方法来处理返回消息】        
// 下面方法：处理IActorResponse 消息，也就是，发回复消息给收消息的人XX, 那么谁发，怎么发，就是这个方法的定义
        // 当是处理【同一进程的消息】：拿到的消息发送器就是当前组件自己，那么只要把结果同步到当前组件的Tcs 异步任务结果里，异步任务结果就会自动触发调用注册过的回调。全部流程结束
        public static void HandleIActorResponse(this ActorMessageSenderComponent self, IActorResponse response) {
            ActorMessageSender actorMessageSender;
// 下面取、实例化 ActorMessageSender 来看，感觉收消息的 rpcId, 与消息发送者 ActorMessageSender 成一一对应关系。上面的Call() 方法里，创建实例化消息发送者就是这么创始垢 
            if (!self.requestCallback.TryGetValue(response.RpcId, out actorMessageSender)) // 这里取不到，是说，这个返回消息的发送已经被处理了？
                return;
            self.requestCallback.Remove(response.RpcId); // 这个有序字典，就成为实时更新：随时添加，随时删除
            Run(actorMessageSender, response); // 写自身组件的；【返回消息】异步任务的结果。只是写好结果了，并没有发送结果呀，去找哪里发送的？
        }
    }
}
