using System;
using System.IO;
using MongoDB.Bson;
namespace ET.Server {
    // 这次，把它看懂，作好笔记，就不会再忘记了。【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
    [Invoke(TimerInvokeType.ActorLocationSenderChecker)]
    // 【位置消息超时、自动检测机制】：这个机制，会自动移除超时了的索要位置的消息，简单暴力，超时了不回馈通知发送端
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
            // 每10s扫描一次过期的actorproxy进行回收,过期时间是2分钟，【它写错了，应该是 1 分钟】
            // 可能由于bug或者进程挂掉，导致ActorLocationSender发送的消息没有确认，结果无法自动删除，每一分钟清理一次这种ActorLocationSender
            // （上面的发送的消息没有确认，没读明白）我说它怎么框架里出一堆为总服分压的分服超时自动检测机制，原来是 bug, 真是弱小。。框架架构师，也有因为【BUG：】不得不重构的时候。。弱弱猫猫。。
            // 看来，亲爱的表哥的活宝妹，还算是心理强大滴～～亲爱的表哥，活宝妹一定要嫁的亲爱的表哥！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！爱表哥，爱生活！！！
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
// 【异步等待挂号排队锁】：最多默认等待1 分钟就回收；它是可以早于 1 分钟回收的，当索要的共享资源的总共排队时间少于1 分钟的时候
// 它是【自动挂号排队锁】，不是【独占锁】，不用考虑代码块的执行时间。
            // 这里不是用锁：对象池里抓个非托管资源，用完自动回收。。
            using (ListComponent<long> list = ListComponent<long>.Create()) { 
                long timeNow = TimeHelper.ServerNow();
                foreach ((long key, Entity value) in self.Children) { // 它遍历这些子组件：去找添加的地方
                    ActorLocationSender actorLocationMessageSender = (ActorLocationSender) value;
                    if (timeNow > actorLocationMessageSender.LastSendOrRecvTime + ActorLocationSenderComponent.TIMEOUT_TIME) 
                        list.Add(key);
                }
                // 如果【被索要位置的、被请求对象】，其专用发送组件代理，60 秒里不活动（60 秒内不曾发送消息或是接收消息），这里是同一进程【位置服】对所管理对象的自动消号处理
                // 当前【全局单例位置管理器】：视其为掉线、下线、搬家过程中，自动移除其位置消息，视为不曾见过不知道。我记得先前读过上各种锁、超时锁，现在全看不见了还是记错了？
                foreach (long id in list) {
                    self.Remove(id);
                }
            }
        }
        private static ActorLocationSender GetOrCreate(this ActorLocationSenderComponent self, long id) { // 拿，或者创建新的
            if (id == 0) 
                throw new Exception($"actor id is 0");
            if (self.Children.TryGetValue(id, out Entity actorLocationSender)) // 有就直接返回：【todo 这里最好是能往回看，看下这个字典怎么管理的】
                return (ActorLocationSender) actorLocationSender;
            // 下面，没有就创建一个新的
            actorLocationSender = self.AddChildWithId<ActorLocationSender>(id); // 当子控件来添加的，不是加入字典里。回收时就需要回收子控件
            return (ActorLocationSender) actorLocationSender;
        }
        private static void Remove(this ActorLocationSenderComponent self, long id) { // 这个字典：值（控件），的非拖管资源，嵌套系统化回收，有点点儿不透彻。。
            if (!self.Children.TryGetValue(id, out Entity actorMessageSender)) return; // 字典里没有？还是说，self.Children[id] ＝ null 不用管？
            // 这里是字典。字典一般需要清除键值对。子控件，只需要子控件回收
            actorMessageSender.Dispose(); // 当前回收的控件：是字典里 id 的值，是一个控件。把这个控件回收了，即self.Children[id] ＝ null;
        }
        // 【发送：请求位置的消息】： entityId, 是【被查询】位置信息的实例 id, 还是【查询】他人位置信息的实例 id? 像是前者，待确认！！
        public static void Send(this ActorLocationSenderComponent self, long entityId, IActorRequest message) {
            self.Call(entityId, message).Coroutine(); // 调用异步方法
        }
        public static async ETTask<IActorResponse> Call(this ActorLocationSenderComponent self, long entityId, IActorRequest iActorRequest) {
            ActorLocationSender actorLocationSender = self.GetOrCreate(entityId); // 封装一个：【位置消息发送代理】
            // 【先序列化好】：位置消息是IActorMessage 里更特异的一小类。但仍属于 IActorMessage. 
            int rpcId = ActorMessageSenderComponent.Instance.GetRpcId(); // ActorMessageSenderComponent 组件：局部变量标记号，自家统筹管理的标记号
            iActorRequest.RpcId = rpcId;
            long actorLocationSenderInstanceId = actorLocationSender.InstanceId; 
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.ActorLocationSender, entityId)) { //entityId: 【查询】与【被查询】，这里感觉锁的是查询发问者，不是锁被查位置的！！
// 检查：【被查询】，的小伙伴，专用【位置消息发送代理】进程ActorId是否超时，是否为重建的？
                if (actorLocationSender.InstanceId != actorLocationSenderInstanceId) 
                    throw new RpcException(ErrorCore.ERR_ActorTimeout, $"{iActorRequest}");
                // 队列中没处理的消息返回跟上个消息一样的报错。【这里写得狠跳跃，明明看懂了，可是感觉不知道这句说什么鬼意思。。】
                // 框架里找不到【被查询小伙伴】，什么时候，发现这个异常写这个Error 的？【掉线下线了】 me.entity ＝ null 或 me 在天涯海角游山走水没背邮箱没注册地址、不能被查位置。。
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
            actorLocationSender.LastSendOrRecvTime = TimeHelper.ServerNow(); // 更新：最后活动时间
            while (true) { // 无限循环：这里只是写法、过程中，一收到异步【位置服】返回来的地址，就返回结果给请求方了
                if (actorLocationSender.ActorId == 0) { // 如果【被查询小伙伴 me】第一次被查位置，专用代理 ActorId ＝ 0
                    actorLocationSender.ActorId = await LocationProxyComponent.Instance.Get(actorLocationSender.Id); // 【位置代理】如何发【跨进程消息】索拿【被查询小伙伴 me】所在进程？的ActorId 的。【跨进程发送与返回消息过程都明白】
                    if (actorLocationSender.InstanceId != instanceId) 
                        throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout2, $"{iActorRequest}");
                }
                // 下面的：line 139 当【被查询位置消息】专用代理超时，也会重罫为 0
                if (actorLocationSender.ActorId == 0) { // 仍为 0, 写出错结果。感觉这里再异步拿一次被查小伙伴的进程地址仍出错，需要去原项目找【位置服】的处理逻辑
                    actorLocationSender.Error = ErrorCore.ERR_NotFoundActor;
                    return ActorHelper.CreateResponse(iActorRequest, ErrorCore.ERR_NotFoundActor); // 掉线、下线、搬家失踪过程中。。。
                }
                // 发送索要位置信息：要求不抛异常。【位置服】进程单例组件，自动移除超时查询消息，不反馈给发送端。发送端收不到返回，自动重试 N次。连试N次拿不到位置，抛异常
                IActorResponse response = await ActorMessageSenderComponent.Instance.Call(actorLocationSender.ActorId, rpcId, iActorRequest, false);
                if (actorLocationSender.InstanceId != instanceId) 
                    throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout3, $"{iActorRequest}");
                switch (response.Error) {
                case ErrorCore.ERR_NotFoundActor: { // 掉线、下线、搬家失踪不能查：132 行的注释：感觉小伙伴搬家过程中，每半秒发一次发20 次，也是比较浪费性能
                        // 如果没找到Actor,重试
                        ++failTimes;
                        if (failTimes > 20) {
                            Log.Debug($"actor send message fail, actorid: {actorLocationSender.Id}");
                            actorLocationSender.Error = ErrorCore.ERR_NotFoundActor;
                            // 这里不能删除actor，要让后面等待发送的消息也返回ERR_NotFoundActor，直到超时删除。（这里超时是指，被查询位置专用发送代理器 60 秒不活动状态）
                            return response; 
                        }
                        // 等待0.5s再发送
                        await TimerComponent.Instance.WaitAsync(500);
                        if (actorLocationSender.InstanceId != instanceId)
                            throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout4, $"{iActorRequest}");
                        actorLocationSender.ActorId = 0; // 这里先重置为0, 上面有逻辑会重新再设置一次  // <<<<<<<<<<<<<<<<<<<< 
                        continue;
                    }
                case ErrorCore.ERR_ActorTimeout: // 发送索要位置信息，要求不抛异常。这里自已断定【被查询位置消息小伙伴】所在进程的 ActorId超时，自己手动抛异常 
                        throw new RpcException(response.Error, $"{iActorRequest}");
                }
                if (ErrorCore.IsRpcNeedThrowException(response.Error)) { // 不懂：这个异步是什么意思？
                    throw new RpcException(response.Error, $"Message: {response.Message} Request: {iActorRequest}");
                }
                return response; // 是返回的正常位置消息，就返回
            }
        }
    }
}
// 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！】
// 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！】