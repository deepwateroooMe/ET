﻿using System;
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
        AddService = 1,     // 添加
        RemoveService = 2,  // 删除
        OnAccept = 3,       // 三大、连接回调
        OnRead = 4,
        OnError = 5,
        CreateChannel = 6, // 建立信道：专用方法，是用来创建新的
        RemoveChannel = 7, // 移除信道
        SendMessage = 9,   // 发送消息
        GetChannelConn = 10, // 只读：现在存在的，不负责创建、任何新的
        ChangeAddress = 11,
    }

    public struct NetOperator { // 上面的【网络操作符】：结构包装体，包装这个【特定的、网络操作符】必要的信息
        public NetOp Op;      // 操作码：几乎唯一标记？是的
        public int ServiceId; // 服务号
        public long ChannelId;// 信道号：【服务号】＋【信道号】＝唯一标记一个【会话框】连接，不标记身哪一端？
        public long ActorId;  // 信使号
        public object Object; // 参数
    }

    // 【这个类】：网络服务，是负责【服务端NetServerComponent】与【客户端NetClientComponnet】交互的中心逻辑部分。双端都有，所以包含双端逻辑；各端按所需要用适合自己的逻辑相关部分
    public class NetServices: Singleton<NetServices> { // 【异步网络交互】：主线程与异步线程。异步线程结果必须同步到主线程上去。否则主线程并不知晓
        private readonly ConcurrentQueue<NetOperator> netThreadOperators = new ConcurrentQueue<NetOperator>(); // 多线程、进程？安全队列
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
		// 这三个：就是主线程，对三类主要网络连接事件的管理逻辑【管理字典】，方便主线程必要的时候查找，调用的。可以把框架翻翻，看看、确认自己想的是对的！！【TODO】：
		// 【TODO】：那个交待过的、单线程多进程模式，还是单进程多线程模式？在这里与，单线程逻辑，有什么区别？
        private readonly Dictionary<int, Action<long, IPEndPoint>> acceptCallback = new Dictionary<int, Action<long, IPEndPoint>>();
        private readonly Dictionary<int, Action<long, long, object>> readCallback = new Dictionary<int, Action<long, long, object>>();
        private readonly Dictionary<int, Action<long, int>> errorCallback = new Dictionary<int, Action<long, int>>();
        private int serviceIdGenerator;
// 【主线程中定义】的几个帮助方法：方法的逻辑里是包装异步任务，交由网络异步线程去完成，最终再把结果同步到主线程上来。也就实现了多线程同步，为主线程分担部分责任
        // 【异步任务】：是异步方法，返回【已经建立的、现在存在的、信道的 reference 索引】，【还是或（或是不存在，必要时）补建信道、或重建信道，并返回信道的 reference? 不是这样的】
        // 【异步方法】：网络操作大多是异步的。这里只是异步去【读取、或拿到】所需要的信道信息。不需要重新的，是GetChannelConn, 不是 CreateChannel
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
        // 【双端都用的单例类】：会话框上发消息：封装为进程间网络异步调用，也就是开启一个异步线程来完成任务，结果同步到主线程中去。
		// 是这样吗，怎么感觉读起来表述并不清楚？要分进程内消息，与跨进程消息吧？进程内就直接信道下发下去底层了；跨进程，才多些步骤？
        public void SendMessage(int serviceId, long channelId, long actorId, object message) { // 这个方法用到的地方狠多，要自己理清思路
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
// 【主线程三大回调逻辑】：是【客户端？一定是客户端吗？服务端呢？】异步线程向【主线程】订阅注册回调。那么Action<X,Y> 是会回调到异步线程【客户端？】中去的。
		// 这里，上面，不一定是客户端，也可以是服务端，更准确应该是异步线程
		// 【网络模块】对三大回调的管理逻辑：加入到自己的管理字典里去，也就是方便主线程必要的时候，调用、回调到异步线程中去
// 框架里找一个：客户端【不一定，或服务端】注册回调的实例使用的例子。网络客户端、内网消息，服务端组件、热更新域里，凡需要拿到服务端回调的地方，都需要向【主线程注册回调】
		// 上面几个部件列举：NetClientComponent, NetInnerComponent, NetServerComponent 等的生成系热更新域里 System.cs
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
// 各线程同步的最顶端同步总管：三大主要事件方法，有如下几个主要主件，向这里？注册。
	// NetInnerComponent NetServerComponent, 也就是ET框架里所有可能的服务端网络组件：【服务端网络组件】与【服务端内网交互网络组件】
	// 各种不同类型服务的底层：KService TService WService
// 事件的流通方向：
	// 这里是座桥：是【异步线程中的服务端】，向主线程同步三大回调。去理，回调的起始、流通方向？OnAccept 感觉基本弄清楚了。另两个事件是一样的原理吗？
		// 网络通信的最底端、三大不同类型的服务：KService TService WService, 当它们接受了任何相对于它们服务端的【客户端】，都会投向、同步到主线程，这里 NetServices;
		// NetServices 主线程，将最底层网络服务端的三大回调，分发回调到【处在，异步线程，的服务端】：NetInnerComponent NetServerComponent
		// 【异步线程中的服务端】：根据主线程回调来的，参数，网络最底层各不同类型服务反馈回来的【所创建的通信信道号】，包装【会话框】，加入到它【异步线程服务端】的客户端的管理字典里管理
					case NetOp.OnAccept: { // channelId 是怎么传进来的：任何客户端与服务端初建通信时，都会创建通信信道，那时就创建好管理着的，必要时拿来直用标记通信的两端的
                            if (!this.acceptCallback.TryGetValue(op.ServiceId, out var action)) // 拿到先前，网络线程客户端，曾经向服务端注册过的回调
                                return; // 客户端注册过的回调，被服务端主线程这里，加字典里记着。不曾注册过什么回调，不用管
                            action.Invoke(op.ChannelId, op.Object as IPEndPoint); // 调用执行回调，真正调用的是网络线程服务端中，他们各客户端？自己的不同的实现方法。不懂就去找注册回调的过程
                            break;
                        }
                    case NetOp.OnRead: { // 【异步线程】同步到了【主线程】这里：
						// 反方向找，异步线程的某端，向主线程注册 readCallback 的地方：凡需要使用网络模块，都会向主线程注册的。网络客户端组件、网络内网组件、网络服务端组件等生成系里
                            if (!this.readCallback.TryGetValue(op.ServiceId, out var action)) {
                                return;
                            }
                            action.Invoke(op.ChannelId, op.ActorId, op.Object); // 调用执行
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
						case NetOp.SendMessage: { // 【会话框上发消息】的最底层：可以追到这里。仅只这里？亲爱的表哥的活宝妹，现在应该能够理解得再深入一点儿了！！
                            AService service = this.Get(op.ServiceId);
                            if (service != null) 
// 再接着就是最底层了。。可以不用弄懂。【服务端】处理好后，消息返回的过程. 那么过程是：远程消息先到达【本进程】服务端，服务端处理本进程返回消息，直接会话框上处理：就是写Tcs 异步结果，异步回【异步回：ETTask 的封装，异步结果返回、写好后的、订阅通知模式？再看一下全忘了】请求方。【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
                                service.Send(op.ChannelId, op.ActorId, op.Object); // 下午：现在，把这个过程再看一遍 
                            break;
                        }
						case NetOp.GetChannelConn: { // 这个方法的细节：
                            var tcs = op.Object as TaskCompletionSource<ValueTuple<uint, uint>>;
                            try { // 就是单一的AService 一种服务类型
                                AService service = this.Get(op.ServiceId);
                                if (service == null) 
                                    break; // 这里断掉后，直接抛 e 异常吗？下面？还是说让它自己等，到时候抛、超时异常？
                                tcs.SetResult(service.GetChannelConn(op.ChannelId)); // 如果信道存在，写结果进封装的异步任务
                            }
                            catch (Exception e) {
                                tcs.SetException(e); // <<<<<<<<<<<<<<<<<<<< 
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
		// 亲爱的表哥的活宝妹，这里，就是感觉，这些事件的流通方向，不明白。要连贯起来看【TODO】：
		// 这里是座桥：是【异步线程中的服务端】，向主线程同步三大回调。去理，回调的起始、流通方向？
		// 硬件、服务器、操作系统的底端，三大不同类型的服务：KService
        public void OnAccept(int serviceId, long channelId, IPEndPoint ipEndPoint) { // 其它【身在异步线程的、服务端】向主线程，同步其与客户端的连接状态？
            NetOperator netOperator = new NetOperator() { Op = NetOp.OnAccept, ServiceId = serviceId, ChannelId = channelId, Object = ipEndPoint };
            this.mainThreadOperators.Enqueue(netOperator); // 投到主线程中去：【让主线程知晓】
        }
        // 公有方法 OnRead(): 哪里会调用这里吗？异步线程的、信道的底层，某端收到读到消息后，回调到【异步线程的】这里；这里再同步到中转，同步到主线程上去
		// 先想明白一个问题：客户端，想要能够网络通信，它能不挂NetService.cs 脚本吗？不能。所以，这个异步线程，可以是服务端，同样也可以是客户端！
        public void OnRead(int serviceId, long channelId, long actorId, object message) { // 【异步线程】：发布？同步？读到消息事件，到【主线程】
            NetOperator netOperator = new NetOperator() { Op = NetOp.OnRead, ServiceId = serviceId, ChannelId = channelId, ActorId = actorId, Object = message };
            this.mainThreadOperators.Enqueue(netOperator); // 扔主线程的队列里去
        }
        public void OnError(int serviceId, long channelId, int error) {
            NetOperator netOperator = new NetOperator() { Op = NetOp.OnError, ServiceId = serviceId, ChannelId = channelId, ActorId = error };
            this.mainThreadOperators.Enqueue(netOperator);
        }
#endregion
#region 主线程kcp id生成
// 这个因为是NetClientComponent中使用，不会与Accept冲突
        public uint CreateConnectChannelId() { // 随机生成一个：信道号——身份证般唯一标识号
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