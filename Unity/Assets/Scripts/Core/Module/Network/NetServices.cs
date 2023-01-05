﻿using System;
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
    
    public enum NetOp: byte {
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
    
    public struct NetOperator {
        public NetOp Op; // 操作码
        public int ServiceId;
        public long ChannelId;
        public long ActorId;
        public object Object; // 参数
    }

// 这里不想读得太细了，简单把TCP弄懂，和它在框架中的前后上下文弄清楚就可以了    
    public class NetServices: Singleton<NetServices> {

// 这里,为什么要管理, 主线程与异步线程? 回调的区分 ? 这个再好好想一下       
        private readonly ConcurrentQueue<NetOperator> netThreadOperators = new ConcurrentQueue<NetOperator>();  // 网络线程
        private readonly ConcurrentQueue<NetOperator> mainThreadOperators = new ConcurrentQueue<NetOperator>(); // 主线程 

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
// 网络连接，调用过程中可能会涉及到的所有回调事件的回调管理          
        private readonly Dictionary<int, Action<long, IPEndPoint>> acceptCallback = new Dictionary<int, Action<long, IPEndPoint>>();
        private readonly Dictionary<int, Action<long, long, object>> readCallback = new Dictionary<int, Action<long, long, object>>();
        private readonly Dictionary<int, Action<long, int>> errorCallback = new Dictionary<int, Action<long, int>>();
        
        private int serviceIdGenerator;
        public async Task<(uint, uint)> GetChannelConn(int serviceId, long channelId) {
            TaskCompletionSource<(uint, uint)> tcs = new TaskCompletionSource<(uint, uint)>();
            NetOperator netOperator = new NetOperator() { Op = NetOp.GetChannelConn, ServiceId = serviceId, ChannelId = channelId, Object = tcs};
            this.netThreadOperators.Enqueue(netOperator); // <<<<<<<<<<<<<<<<<<<< 在异步线程管理队列中入队，主要是方便  当前  网络管理服务器 NetServices 管理操作方便？
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
        public void UpdateInMainThread() { // 一个一个地遍历  注册到过 主线程中的操作，正当合法，就触发必要的回调给各相关（或一对一，或一对多客户端？）客户端  
            while (true) {
                if (!this.mainThreadOperators.TryDequeue(out NetOperator op)) {
                    return;
                }
                try {
                    switch (op.Op) {
                    case NetOp.OnAccept: { // <<<<<<<<<<<<<<<<<<<< OnAccept 
                            if (!this.acceptCallback.TryGetValue(op.ServiceId, out var action)) {
                                return;
                            }
                            action.Invoke(op.ChannelId, op.Object as IPEndPoint); // <<<<<<<<<< 
                            break;
                        }
                    case NetOp.OnRead: { // <<<<<<<<<<<<<<<<<<<< OnRead
                            if (!this.readCallback.TryGetValue(op.ServiceId, out var action)) {
                                return;
                            }
                            action.Invoke(op.ChannelId, op.ActorId, op.Object); // <<<<<<<<<< 
                            break;
                        }
                    case NetOp.OnError: { // <<<<<<<<<<<<<<<<<<<< OnError
                            if (!this.errorCallback.TryGetValue(op.ServiceId, out var action)) {
                                return;
                            }
                            action.Invoke(op.ChannelId, (int)op.ActorId); // <<<<<<<<<< 
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
// 实现了网络交互过程中的操作: 添加移除服务,创建移除信道,发送消息,获取连接等
        private void RunNetThreadOperator() { // 这里的问题成为:当网络线程为多个不同的客户端异步线程，为什么什么情况下  这些注册过的操作  会统一调用处理？每秒处理一次 OnTick()之类的时候？  
            while (true) { // 周期性  机械性  遍历  异步线程操作 
                if (!this.netThreadOperators.TryDequeue(out NetOperator op)) {
                    return;
                }
                try {
                    switch (op.Op) {
// AddService + RemoveService                        
                        case NetOp.AddService: {
                            this.Add(op.Object as AService);
                            break;
                        }
                        case NetOp.RemoveService: {
                            this.Remove(op.ServiceId);
                            break;
                        }
// CreateChannel + RemoveChannel                            
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
// SendMessage                            
                        case NetOp.SendMessage: {
                            AService service = this.Get(op.ServiceId);
                            if (service != null) {
                                service.Send(op.ChannelId, op.ActorId, op.Object);
                            }
                            break;
                        }
// GetChannelConn                            
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
// ChangeAddress                            
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
// <<<<<<<<<<<<<<<<<<<< ???        
        public void UpdateInNetThread() { // 不明白:这个方法到底是什么意思,什么情况下需要调用 ?
            int count = this.queue.Count;
            while (count-- > 0) {
                int serviceId = this.queue.Dequeue();
                if (!this.services.TryGetValue(serviceId, out AService service)) { // AService: 是单例模式吗?
                    continue;
                }
                this.queue.Enqueue(serviceId); // 不曾注册的,注册上
                service.Update(); // <<<<<<<<<< 去找个实现方法,读一下
            }
            
            this.RunNetThreadOperator();
        }
        public void OnAccept(int serviceId, long channelId, IPEndPoint ipEndPoint) {
            NetOperator netOperator = new NetOperator() { Op = NetOp.OnAccept, ServiceId = serviceId, ChannelId = channelId, Object = ipEndPoint };
            this.mainThreadOperators.Enqueue(netOperator);
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