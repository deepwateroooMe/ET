using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
namespace ET {
	// 【网络模块】：小项目或是大项目，不管多大的项目，都需要能够如亲爱的表哥的活宝妹今天这样，把前后需要的逻辑，一定完全全部找出来。要具备无条件阅读读懂任何大型项目相关必要源码的能力！
    public enum NetworkProtocol {
        TCP,
        KCP, // 内网组件：用的是 KCP 以前没细看，找下不同类型，各用在什么地方
        Websocket,
    }
    public enum NetOp: byte { // 几、网络操作码，类型，分类管理
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
    public struct NetOperator { // 结构体
        public NetOp Op; // 操作码
        public int ServiceId;
        public long ChannelId;
        public long ActorId;
        public object Object; // 参数
    }
    public class NetServices: Singleton<NetServices>, ISingletonUpdate { // 双端【单线程多进程】
        private readonly ConcurrentQueue<NetOperator> netThreadOperators = new ConcurrentQueue<NetOperator>();
        private readonly ConcurrentQueue<NetOperator> mainThreadOperators = new ConcurrentQueue<NetOperator>();
        public NetServices() { 
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (MessageAttribute));
            foreach (Type type in types) { // Protobuf 里【内网、外网消息】不同消息类型，与网络操作码，启动时程序域里高效快速扫
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
#if !SINGLE_THREAD // 添加了多线程支持？
            // 网络线程
            this.thread = new Thread(this.NetThreadUpdate);
            this.thread.Start();
#endif
        }
        public override void Dispose() {
#if !SINGLE_THREAD
            this.isStop = true;            
            this.thread.Join(1000);
#endif
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
        private readonly Dictionary<int, Action<long, IPEndPoint>> acceptCallback = new Dictionary<int, Action<long, IPEndPoint>>();
        private readonly Dictionary<int, Action<long, long, object>> readCallback = new Dictionary<int, Action<long, long, object>>();
        private readonly Dictionary<int, Action<long, int>> errorCallback = new Dictionary<int, Action<long, int>>();
        private int serviceIdGenerator;
		// 主线程【单线程多进程】的逻辑是：把投向主线程的各任务，包装分装进各种不同的【网络异步线程】里去分压。
		// 主线程，怎么就到异步线程去了？是相当于要求多线程处理呀，就是开个线程去干耗时的网络异步请求之类的，所以会去异步线程。这里可以再多想想
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
		// 三大网络组件、或者说，使用网络模块的几个端：NetClientComponent, NetInnerComponent, NetServerComponent 客户端、服务端、内网端
		// 但凡添加网络组件【NetService】都会向其【某端：客户端、或是服务端】单例 NetService.Instance 注册以下三大回调
        public void RegisterAcceptCallback(int serviceId, Action<long, IPEndPoint> action) {
            this.acceptCallback.Add(serviceId, action);
        }
        public void RegisterReadCallback(int serviceId, Action<long, long, object> action) {
            this.readCallback.Add(serviceId, action);
        }
        public void RegisterErrorCallback(int serviceId, Action<long, int> action) {
            this.errorCallback.Add(serviceId, action);
        }
        private void UpdateInMainThread() {
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
						case NetOp.OnRead: { // 信道上读到消息：同步到主线程这里、桢里执行
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
#if !SINGLE_THREAD
        private bool isStop;
        private readonly Thread thread;
        // 网络线程Update
        private void NetThreadUpdate() {
            while (!this.isStop) {
                this.UpdateInNetThread();
                Thread.Sleep(1);
            }
        }
#endif
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
							if (service != null) {
								service.Remove(op.ChannelId, (int)op.ActorId);
							}
							break;
						}
						case NetOp.SendMessage: { // 不再细看一遍了
							AService service = this.Get(op.ServiceId);
							if (service != null) { // 实体服务：将消息发出去，走【信道 ==> Socket 等】向下向底层、内存流上、发送序列化后消息的过程
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
        private void UpdateInNetThread() {
            int count = this.queue.Count;
            while (count-- > 0) {
                int serviceId = this.queue.Dequeue();
                if (!this.services.TryGetValue(serviceId, out AService service)) {
                    continue;
                }
                this.queue.Enqueue(serviceId);
                service.Update();
            }
            this.RunNetThreadOperator();
        }
        public void OnAccept(int serviceId, long channelId, IPEndPoint ipEndPoint) {
            NetOperator netOperator = new NetOperator() { Op = NetOp.OnAccept, ServiceId = serviceId, ChannelId = channelId, Object = ipEndPoint };
            this.mainThreadOperators.Enqueue(netOperator);
        }
		// 【信道】上调用的、双端某端的【异步线程】读到消息：先同步到主线程上去
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
        public void Update() {
#if SINGLE_THREAD
            UpdateInNetThread();
#endif
            UpdateInMainThread();
        }
    }
}
