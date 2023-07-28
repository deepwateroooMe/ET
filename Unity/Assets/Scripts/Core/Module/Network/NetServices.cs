using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
namespace ET {
    // 进程间通信，网络交互的基础类
    public enum NetworkProtocol { // 服务的几种类型
        TCP,
        KCP,
        Websocket,
    }
    public enum NetOp: byte { // 定义几种不同的网络操作
        AddService = 1,
        RemoveService = 2,
        OnAccept = 3,
        OnRead = 4,
        OnError = 5,
        CreateChannel = 6,
        RemoveChannel = 7,
        SendMessage = 9,
        GetChannelConn = 10,
        ChangeAddress = 11,
    }

    public struct NetOperator { // 上面的【网络操作符】：结构包装体，包装这个【特定的、网络操作符】必要的信息
        public NetOp Op; // 操作码
        public int ServiceId;
        public long ChannelId;
        public long ActorId;
        public object Object; // 参数
    }

    // 【这个类】：网络服务，是负责【服务端NetServerComponent】与【客户端NetClientComponnet】交互的中心逻辑部分
    public class NetServices: Singleton<NetServices> { // 【异步网络交互】：主线程与异步线程。异步线程结果必须同步到主线程上去。否则主线程并不知晓
        private readonly ConcurrentQueue<NetOperator> netThreadOperators = new ConcurrentQueue<NetOperator>();
        private readonly ConcurrentQueue<NetOperator> mainThreadOperators = new ConcurrentQueue<NetOperator>();

        public NetServices() { // 【原理】：扫描框架里所有消息，把消息的操作符记字典里
            // Proto2CS: 当 .proto 里自定义的消息，框架里转化为 .cs 语言消息时，会为消息分配【网络操作符】数字，作为这里 messageAttribute.Opcode
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (MessageAttribute)); // 【进程间消息】【Message(OuterMessage.RouterSync)】 etc
            foreach (Type type in types) {
                object[] attrs = type.GetCustomAttributes(typeof (MessageAttribute), false);
                if (attrs.Length == 0)
                    continue;
                MessageAttribute messageAttribute = attrs[0] as MessageAttribute;
                if (messageAttribute == null)
                    continue;
                // 
                this.typeOpcode.Add(type, messageAttribute.Opcode); // 【初始化】：扫描框架里所有消息。每个消息，类型，与操作符是固定不变的
            }
        }
#region 线程安全
// 初始化后不变，所以主线程，网络线程都可以读
// 【初始化与赋值】：在上面，打描框架里所有消息。每个消息，类型，与操作符是固定不变的
        private readonly DoubleMap<Type, ushort> typeOpcode = new DoubleMap<Type, ushort>(); 
        public ushort GetOpcode(Type type) {
            return this.typeOpcode.GetValueByKey(type);
        }
        public Type GetType(ushort opcode) {
            return this.typeOpcode.GetKeyByValue(opcode);
        }
#endregion
#region 主线程
        private readonly Dictionary<int, Action<long, IPEndPoint>> acceptCallback = new Dictionary<int, Action<long, IPEndPoint>>();
        private readonly Dictionary<int, Action<long, long, object>> readCallback = new Dictionary<int, Action<long, long, object>>();
        private readonly Dictionary<int, Action<long, int>> errorCallback = new Dictionary<int, Action<long, int>>();
        private int serviceIdGenerator;
// 【主线程中定义】的几个帮助方法：方法的逻辑里是包装异步任务，交由网络异步线程去完成，最终再把结果同步到主线程上来
        // 【异步任务】：是异步方法，但是这里是，返回已经建立的信道的 reference 索引，还是重新（或是不存在，必要时）或补建信道，并返回信道的 reference ？
        // 【异步方法】：网络操作大多是异步的。这里只是异步去【读取、或拿到】所需要的信道信息。应该是不需要重新的，是GetChannelConn, 不是 CreateChannel
// 【分派到异步线程】去处理的方法：各方法逻辑，负责封装加入到异步线程待处理队列中去；异步线程会去执行其队列中的异步任务
        public async Task<(uint, uint)> GetChannelConn(int serviceId, long channelId) {
            TaskCompletionSource<(uint, uint)> tcs = new TaskCompletionSource<(uint, uint)>();
            NetOperator netOperator = new NetOperator() { Op = NetOp.GetChannelConn, ServiceId = serviceId, ChannelId = channelId, Object = tcs};
            this.netThreadOperators.Enqueue(netOperator);
            return await tcs.Task;
        }
        public void ChangeAddress(int serviceId, long channelId, IPEndPoint ipEndPoint) {
            NetOperator netOperator = new NetOperator() { Op = NetOp.ChangeAddress, ServiceId = serviceId, ChannelId = channelId, Object = ipEndPoint};
            this.netThreadOperators.Enqueue(netOperator);
        }
        // 【双端都用的单例类】：会话框上发消息：封装为进程间网络异步调用，也就是开启一个异步线程来完成任务，结果同步到主线程中去
        public void SendMessage(int serviceId, long channelId, long actorId, object message) {
            NetOperator netOperator = new NetOperator() { Op = NetOp.SendMessage, ServiceId = serviceId, ChannelId = channelId, ActorId = actorId, Object = message };
            this.netThreadOperators.Enqueue(netOperator);
        }
        public int AddService(AService aService) {
            aService.Id = ++this.serviceIdGenerator;
            NetOperator netOperator = new NetOperator() { Op = NetOp.AddService, ServiceId = aService.Id, ChannelId = 0, Object = aService };
            this.netThreadOperators.Enqueue(netOperator);
            return aService.Id;
        }
        public void RemoveService(int serviceId) {
            NetOperator netOperator = new NetOperator() { Op = NetOp.RemoveService, ServiceId = serviceId };
            this.netThreadOperators.Enqueue(netOperator);
        }
        public void RemoveChannel(int serviceId, long channelId, int error) {
            NetOperator netOperator = new NetOperator() { Op = NetOp.RemoveChannel, ServiceId = serviceId, ChannelId = channelId, ActorId = error};
            this.netThreadOperators.Enqueue(netOperator);
        }
        public void CreateChannel(int serviceId, long channelId, IPEndPoint address) {
            NetOperator netOperator = new NetOperator() { Op = NetOp.CreateChannel, ServiceId = serviceId, ChannelId = channelId, Object = address};
            this.netThreadOperators.Enqueue(netOperator);
        }
// 【主线程三大回调逻辑】：可想而知，应该是【客户端】异步线程向【主线程】订阅注册回调。那么Action<X,Y> 是会回调到异步线程【客户端】中去的。（这里不一定是客户端，更准确应该是异步线程）
// 框架里找一个：客户端注册回调的实例使用的例子。网络客户端、内网消息，凡需要拿到服务端回调的地方，如同Demon-client 模式，都需要向服务端注册 
        public void RegisterAcceptCallback(int serviceId, Action<long, IPEndPoint> action) { 
            this.acceptCallback.Add(serviceId, action);
        }
        public void RegisterReadCallback(int serviceId, Action<long, long, object> action) { // 这个回调管理的前前后后，再多看几遍
            this.readCallback.Add(serviceId, action); // 向本【单例管理类】：注册读到消息的回调事件。是本处读到消息，回调给这里注册过的回调
        }
        public void RegisterErrorCallback(int serviceId, Action<long, int> action) {
            this.errorCallback.Add(serviceId, action);
        }
        public void UpdateInMainThread() {
            while (true) {
                if (!this.mainThreadOperators.TryDequeue(out NetOperator op)) { // 【主线程：】队列中没有任务了
                    return;
                }
                try {
// 不同的操作符，传入不同的参数。不同操作符的 action 回调，不同的回调类型与参数，是定义在上面，【主线程的字典回调管理里】
                    switch (op.Op) { // 【主线程中】：只处理了最主要的【必须主线程执行的】三类回调。回调的注册方法在前面。【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！】
                        case NetOp.OnAccept: {
                            if (!this.acceptCallback.TryGetValue(op.ServiceId, out var action)) // 拿到先前，网络线程客户端，曾经向服务端注册过的回调
                                return; // 客户端注册过的回调，被服务端主线程这里，加字典里记着
                            action.Invoke(op.ChannelId, op.Object as IPEndPoint); // 这里回调，真正调用的是网络线程中，他们各客户端自己的不同的实现方法
                            break;
                        }
                    case NetOp.OnRead: { // 同步到【主线程】：反方向找，异步线程的发布读事件
                            if (!this.readCallback.TryGetValue(op.ServiceId, out var action)) {
                                return;
                            }
                            action.Invoke(op.ChannelId, op.ActorId, op.Object);
                            break;
                        }
                        case NetOp.OnError: {
                            if (!this.errorCallback.TryGetValue(op.ServiceId, out var action)) {
                                return;
                            }
                            action.Invoke(op.ChannelId, (int)op.ActorId);
                            break;
                        }
                        default:
                            throw new Exception($"not found net operator: {op.Op}");
                    }
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
#endregion
        
#region 网络线程
        private readonly Dictionary<int, AService> services = new Dictionary<int, AService>();
        private readonly Queue<int> queue = new Queue<int>();
        private void Add(AService aService) {
            this.services[aService.Id] = aService;
            this.queue.Enqueue(aService.Id);
        }
        public AService Get(int id) {
            AService aService;
            this.services.TryGetValue(id, out aService);
            return aService;
        }
        private void Remove(int id) {
            if (this.services.Remove(id, out AService service)) {
                service.Dispose();
            }
        }
        private void RunNetThreadOperator() {
            while (true) {
                if (!this.netThreadOperators.TryDequeue(out NetOperator op)) {
                    return;
                }
                try {
                    switch (op.Op) {
                        case NetOp.AddService: {
                            this.Add(op.Object as AService);
                            break;
                        }
                        case NetOp.RemoveService: {
                            this.Remove(op.ServiceId);
                            break;
                        }
                        case NetOp.CreateChannel: {
                            AService service = this.Get(op.ServiceId);
                            if (service != null) {
                                service.Create(op.ChannelId, op.Object as IPEndPoint);
                            }
                            break;
                        }
                        case NetOp.RemoveChannel: {
                            AService service = this.Get(op.ServiceId);
                            if (service != null) 
                                service.Remove(op.ChannelId, (int)op.ActorId);
                            break;
                        }
                    case NetOp.SendMessage: { // 【会话框上发消息】的最底层：可以追到这里。
                            AService service = this.Get(op.ServiceId);
                            if (service != null) 
// 再接着就是最底层了。。可以不用弄懂。【服务端】处理好后，消息返回的过程. 那么过程是：远程消息先到达【本进程】服务端，服务端处理本进程返回消息，直接会话框上处理：就是写Tcs 异步结果，异步回请求方。【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
                                service.Send(op.ChannelId, op.ActorId, op.Object); 
                            break;
                        }
                        case NetOp.GetChannelConn: {
                            var tcs = op.Object as TaskCompletionSource<ValueTuple<uint, uint>>;
                            try {
                                AService service = this.Get(op.ServiceId);
                                if (service == null) 
                                    break;
                                tcs.SetResult(service.GetChannelConn(op.ChannelId));
                            }
                            catch (Exception e) {
                                tcs.SetException(e);
                            }
                            break;
                        }
                        case NetOp.ChangeAddress: {
                            AService service = this.Get(op.ServiceId);
                            if (service == null) 
                                break;
                            service.ChangeAddress(op.ChannelId, op.Object as IPEndPoint);
                            break;
                        }
                        default:
                            throw new Exception($"not found net operator: {op.Op}");
                    }
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        // 【异步线程】每桢更新逻辑：异步线程，每桢就遍历那几个服务 id(Service 层面， id 相区分), 要每个服务去更新它们自己的
        public void UpdateInNetThread() { // 往上找一下：Update() 生命周期回调，哪里调用这个方法？也是公用方法，怎么会没有调用的地方呢？
            int count = this.queue.Count;
            while (count-- > 0) {
                int serviceId = this.queue.Dequeue();
                if (!this.services.TryGetValue(serviceId, out AService service)) 
                    continue;
                this.queue.Enqueue(serviceId); 
                service.Update(); // 【每个具体服务层面】：调用它们各自服务实现里的Update()
            }
            this.RunNetThreadOperator();
        }
// 【三大主要事件方法】：前面，主线程封装异步任务到网络线程中来执行；现在网络线程，把三大主要回调、封装、投放、同步到主线程队列中去执行？而主线程中的逻辑，仍然是回调到异步线程相应的服务中去调用回调，执行，比如某个客户端服务被主线程？被服务端接收，客户端服务？会去执行相应的OnAccept 功能的回调。就是多了步把结果【同步到主线程】的过程
// 这里服务端可能也在异步线程（因为并存多个或类型相同或类型不同的Service, 每个Service 执行【服务端客户端交互】，这里每个 Service 应该都是客户端？）。
// 不是说服务端接受了某个客户端，主线程就一定知道哪个服务端接受了某个客户端的结果，服务端也在异步线程的话，不投递到主线程，异步线程的结果主线程不知道
// 【投到主线程】：前个游戏的ET-EUI 服务端与不用ET 框架的客户端交互的时候，曾经出现过主线程不知道，客户端使用第三方库来实现同步到主线程中
        public void OnAccept(int serviceId, long channelId, IPEndPoint ipEndPoint) {
            NetOperator netOperator = new NetOperator() { Op = NetOp.OnAccept, ServiceId = serviceId, ChannelId = channelId, Object = ipEndPoint };
            this.mainThreadOperators.Enqueue(netOperator); // 投到主线程中去：【让主线程知晓】。可是去看主线程Update() 里的执行调用。。
        }
        // 公有方法 OnRead(): 哪里会调用这里吗？
        public void OnRead(int serviceId, long channelId, long actorId, object message) { // 【异步线程】：发布？同步？读到消息事件，到【主线程】
            NetOperator netOperator = new NetOperator() { Op = NetOp.OnRead, ServiceId = serviceId, ChannelId = channelId, ActorId = actorId, Object = message };
            this.mainThreadOperators.Enqueue(netOperator);
        }
        public void OnError(int serviceId, long channelId, int error) {
            NetOperator netOperator = new NetOperator() { Op = NetOp.OnError, ServiceId = serviceId, ChannelId = channelId, ActorId = error };
            this.mainThreadOperators.Enqueue(netOperator);
        }
#endregion
#region 主线程kcp id生成
// 这个因为是NetClientComponent中使用，不会与Accept冲突
        public uint CreateConnectChannelId() {
            return RandomGenerator.RandUInt32();
        }
#endregion
#region 网络线程kcp id生成
// 防止与内网进程号的ChannelId冲突，所以设置为一个大的随机数
        private uint acceptIdGenerator = uint.MaxValue;
        public uint CreateAcceptChannelId() {
            return --this.acceptIdGenerator;
        }
#endregion
    }
}