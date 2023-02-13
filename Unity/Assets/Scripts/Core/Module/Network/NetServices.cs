using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ET {

    public enum NetworkProtocol {
        TCP,
        KCP,
        Websocket,
    }
    // 网络操作码： 1个字节   
    public enum NetOp: byte {
        // 添加 移除 服务
        AddService = 1,    
        RemoveService = 2, 
        // 三种类型的回调        
        OnAccept = 3,
        OnRead = 4,
        OnError = 5,
        // 添加 移除 信道 
        CreateChannel = 6,
        RemoveChannel = 7,
        // 其它
        SendMessage = 9,     // 发消息
        GetChannelConn = 10, // 获取可用的信道
        ChangeAddress = 11,  // 改变地址
    }
    
    public struct NetOperator {
        public NetOp Op; // 操作码
        public int ServiceId;
        public long ChannelId;
        public long ActorId;
        public object Object; // 参数
    }

// 这里不想读得太细了，简单把TCP弄懂，和它在框架中的前后上下文弄清楚就可以了    
    public class NetServices: Singleton<NetServices> {

// 这里,为什么要管理, 主线程与异步线程(它们回调,比如Update()的机理都不一样,调用时机好像与不一样)? 回调的区分 ? 网络调用有个方向问题： 有些是服务器回调给客户端,有些是客户端回调给服务器,这里还是没有想透,有点儿糊
// 游戏框架设计是单线程多进程： 主线程可以理解为游戏的这个单线程主线程所在，网络线程就是开启的一个异步线程.网络连接中总是有两个端之间相连，把需要单线程逻辑处理的放主线程里，其它所有不同客户端的全放网络线程里统一管理
// 只是不知道上面理解的对不对？
        // 使用上的区别在于： NetThreadComponentSystem 中，两个不同同步队伍的更新机制不一样
        // 网络线程同步队列：它被开执行在一个异步线程里，每隔1秒钟，就自动运行更新一遍
        // 主线程同步队列：它被执行在游戏引擎同步的生命周期回调函数LateUpdate() 里，所以是每桢都会在主线程执行一次
        private readonly ConcurrentQueue<NetOperator> netThreadOperators = new ConcurrentQueue<NetOperator>();  // 网络线程 
        private readonly ConcurrentQueue<NetOperator> mainThreadOperators = new ConcurrentQueue<NetOperator>(); // 主线程 同步队列

        public NetServices() {
            // 拿到标记过自定义属性MessageAttribute的所有的类class: 用于对双端消息的进一步管理
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (MessageAttribute));
            foreach (Type type in types) {
                object[] attrs = type.GetCustomAttributes(typeof (MessageAttribute), false);
                if (attrs.Length == 0) {
                    continue;
                }
                MessageAttribute messageAttribute = attrs[0] as MessageAttribute;
                if (messageAttribute == null) {
                    continue;
                }
                this.typeOpcode.Add(type, messageAttribute.Opcode);
            }
        }

#region 线程安全
        // 初始化后不变，所以主线程，网络线程都可以读
        private readonly DoubleMap<Type, ushort> typeOpcode = new DoubleMap<Type, ushort>();
        public ushort GetOpcode(Type type) {
            return this.typeOpcode.GetValueByKey(type);
        }
        public Type GetType(ushort opcode) {
            return this.typeOpcode.GetKeyByValue(opcode);
        }
#endregion       
        
#region 主线程
// 主连接，调用过程中可能会涉及到的所有回调事件的回调管理.主要适用于各种不同的服务器，方便它们对它们的客户端连接等进行管理
        private readonly Dictionary<int, Action<long, IPEndPoint>> acceptCallback = new Dictionary<int, Action<long, IPEndPoint>>();
        private readonly Dictionary<int, Action<long, long, object>> readCallback = new Dictionary<int, Action<long, long, object>>();
        private readonly Dictionary<int, Action<long, int>> errorCallback = new Dictionary<int, Action<long, int>>();
        private int serviceIdGenerator;
        // 与主线程单线程逻辑进行通信交互的（或是理解为多个不同的服务器端？），都是网络线程多个不同的客户端，所以主线程的这个力的相互作用的另一端的处理逻辑全部放进网络线程的同步队列里去处理?         
        public async Task<(uint, uint)> GetChannelConn(int serviceId, long channelId) {
            TaskCompletionSource<(uint, uint)> tcs = new TaskCompletionSource<(uint, uint)>();
            NetOperator netOperator = new NetOperator() { Op = NetOp.GetChannelConn, ServiceId = serviceId, ChannelId = channelId, Object = tcs};
            this.netThreadOperators.Enqueue(netOperator); // 网络调用是双向的，这里更多的去想，是哪个端，客户端？更需要获取通信信道，与服务器取得联系？是客户端就放网络线程的同步队列待处理
            return await tcs.Task;
        }
        public void ChangeAddress(int serviceId, long channelId, IPEndPoint ipEndPoint) {
            NetOperator netOperator = new NetOperator() { Op = NetOp.ChangeAddress, ServiceId = serviceId, ChannelId = channelId, Object = ipEndPoint};
            this.netThreadOperators.Enqueue(netOperator);
        }
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
// 管理的是：网络线程们向主线程注册过的各种相关的回调函数，主要三种，被服务器接受连接连接建立时的回调，客户端发送的消息被主服务器读到的回调，以及出错的回调？
// 搜索使用上下文，有NetServerComponentSystem + NetInnerComponentShystem 在使用它们
        public void RegisterAcceptCallback(int serviceId, Action<long, IPEndPoint> action) {
            this.acceptCallback.Add(serviceId, action);
        }
        public void RegisterReadCallback(int serviceId, Action<long, long, object> action) {
            this.readCallback.Add(serviceId, action);
        }
        public void RegisterErrorCallback(int serviceId, Action<long, int> action) {
            this.errorCallback.Add(serviceId, action);
        }
// 这里体现出：作为一个设计精良的框架，设计和实现上对分类事件的管理        
// 一个一个地遍历  注册到过 主线程中的操作，正当合法，就触发必要的回调【给各相关（一对一）的客户端  这里写得不对】主线程单线程逻辑，是立即触发，执行注册过的回调，而不是再返回给客户端什么的
        public void UpdateInMainThread() { 
            while (true) {
                if (!this.mainThreadOperators.TryDequeue(out NetOperator op)) {
                    return;
                }
                try {
                    switch (op.Op) {
                    case NetOp.OnAccept: {
                            if (!this.acceptCallback.TryGetValue(op.ServiceId, out var action)) {
                                return;
                            }
                            action.Invoke(op.ChannelId, op.Object as IPEndPoint); 
                            break;
                        }
                    case NetOp.OnRead: { 
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
// 实现了网络交互过程中的操作: 添加移除服务,创建移除信道,发送消息,获取连接等，就是前面定义过的几种不同操作码的处理
        private void RunNetThreadOperator() { 
            while (true) { // 周期性  机械性  遍历  异步线程操作 
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
                            if (service != null) {
                                service.Remove(op.ChannelId, (int)op.ActorId);
                            }
                            break;
                        }
                        case NetOp.SendMessage: {
                            AService service = this.Get(op.ServiceId);
                            if (service != null) {
                                service.Send(op.ChannelId, op.ActorId, op.Object);
                            }
                            break;
                        }
                        case NetOp.GetChannelConn: {
                            var tcs = op.Object as TaskCompletionSource<ValueTuple<uint, uint>>;
                            try {
                                AService service = this.Get(op.ServiceId);
                                if (service == null) {
                                    break;
                                }
                                tcs.SetResult(service.GetChannelConn(op.ChannelId));
                            }
                            catch (Exception e) {
                                tcs.SetException(e);
                            }
                            break;
                        }
                        case NetOp.ChangeAddress: {
                                AService service = this.Get(op.ServiceId);
                                if (service == null) {
                                    break;
                                }
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
// <<<<<<<<<<<<<<<<<<<< 在异步线程中执行的函数逻辑，每1毫秒都会执行一次. NetThreadComponentSystem一旦Awake() 开启了一个专用的网络线程之后，就会如何周期执行，直到网络线程组件拆解掉   
        public void UpdateInNetThread() {
            // 两大周期性的任务：             
            int count = this.queue.Count; // 一，处理当前队列里服务周期性回调
            while (count-- > 0) {
                int serviceId = this.queue.Dequeue();
                if (!this.services.TryGetValue(serviceId, out AService service)) { // 无效的,就从队列里清除了
                    continue;
                }
                this.queue.Enqueue(serviceId); // 这里是轮循: 像Unity Component的Update()一样,就是调用了service.Update(),再把有效的serviceId给重新放回队列中去 
                service.Update(); // <<<<<<<<<< TODO: 去找个实现方法,读一下，意思是说，这个服务也是有，如抽象蕨类所定义的，如游戏引擎里的生命周期函数Update() 一样周期性地被调用，所以上面会重新再放回去，等待下一桢？
            }
            this.RunNetThreadOperator(); // 二，字典里缓存的操作码也是需要周期性处理的
        }
        public void OnAccept(int serviceId, long channelId, IPEndPoint ipEndPoint) {
            // 创建一个网络通信操作符结构体：
            NetOperator netOperator = new NetOperator() { Op = NetOp.OnAccept, ServiceId = serviceId, ChannelId = channelId, Object = ipEndPoint };
            this.mainThreadOperators.Enqueue(netOperator); // 加在主线程的队列里？
        }
        public void OnRead(int serviceId, long channelId, long actorId, object message) {
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