using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
namespace ET {
    // 这个类：之前，【客户端】会话框上发回复消息的底层原理没能看懂。这个部分要再多看几遍
    public readonly struct RpcInfo { // 【消息】包装体：可以是进程内的。它包装的是基类接口，也适用于所有的继承类接口 RpcMessage RpcLocationMsg-etc
        public readonly IRequest Request;
        public readonly ETTask<IResponse> Tcs;
        public RpcInfo(IRequest request) { 
            this.Request = request;
// 【Tcs 桥接功能异步任务的框架封装】：Tcs 异步任务，这里会话框上的封装，就是底层封装，这里会话框底层封装的作用，就是实现返回消息的桥接回发送端。去细追理解过程细节
            this.Tcs = ETTask<IResponse>.Create(true);
        }
    }
    [FriendOf(typeof(Session))]
    public static class SessionSystem {
        [ObjectSystem]
        public class SessionAwakeSystem: AwakeSystem<Session, int> {
            protected override void Awake(Session self, int serviceId) { // 【会话框创建】：会调用一次
                self.ServiceId = serviceId;
                long timeNow = TimeHelper.ClientNow();
                self.LastRecvTime = timeNow; // 记录：初始化活动时间
                self.LastSendTime = timeNow;
                self.requestCallbacks.Clear(); // 字典是什么时间创建的？基类里生成实例就自动初始化 
                Log.Info($"session create: zone: {self.DomainZone()} id: {self.Id} {timeNow} ");
            }
        }
        [ObjectSystem]
        public class SessionDestroySystem: DestroySystem<Session> {
            protected override void Destroy(Session self) {
                NetServices.Instance.RemoveChannel(self.ServiceId, self.Id, self.Error); // 先取消：从服务总管单例处的注册（否则它会持有当前会话框的索引）
                foreach (RpcInfo responseCallback in self.requestCallbacks.Values.ToArray()) { // 对于所有注册过的回调：一一抛异常，会话框销毁了。。
                    responseCallback.Tcs.SetException(new RpcException(self.Error, $"session dispose: {self.Id} {self.RemoteAddress}"));
                }
                Log.Info($"session dispose: {self.RemoteAddress} id: {self.Id} ErrorCode: {self.Error}, please see ErrorCode.cs! {TimeHelper.ClientNow()}");
                self.requestCallbacks.Clear(); // 清理所有回调：这个字典是永久长存的？？
            }
        }
        public static void OnResponse(this Session self, IResponse response) { // 会话框上发IRequest 消息时，是如何注册回调到会话框的管理字典的？ line 57, 注册回调 
            if (!self.requestCallbacks.TryGetValue(response.RpcId, out var action)) // 从【会话框】层的字典取先前注册过的该 RpcId 的回调
                return;
            self.requestCallbacks.Remove(response.RpcId); // 字典管理. 【下面】：如果有异常，写进Tcs 里，框架（异步任务模块）是会自动抛出异常的，不用再多管了
            if (ErrorCore.IsRpcNeedThrowException(response.Error)) {
                action.Tcs.SetException(new Exception($"Rpc error, request: {action.Request} response: {response}"));
                return;
            }
// 写结果: 这里的Tcs 可能还是起桥接作用。前天晚上终于看懂一个【回复消息】的。这里索要返回消息的【发送消息】过程，与对应于此的【返回消息】的过程，细看一遍
            action.Tcs.SetResult(response); // <<<<<<<<<<<<<<<<<<<< 
        }
        public static async ETTask<IResponse> Call(this Session self, IRequest request, ETCancellationToken cancellationToken) {
            int rpcId = ++Session.RpcId; // 会话框上引入的局部变量：帮助会话框，管理区分不同消息的回调。本地变量 
            RpcInfo rpcInfo = new RpcInfo(request); // 这里，自动封装入一个Tcs 用来回调返回消息
            self.requestCallbacks[rpcId] = rpcInfo; // 【会话框上注册发送消息的回调】：注册
// 管理系统的【身份证】索引号：这里写入【发送消息】；同样的值，将会被写到【返回消息】；将会根据【返回消息】的这个号，来这里管理系索要注册过的回调，【并调用回调（？将返回消息写回去）？】
            request.RpcId = rpcId; 
            self.Send(request);
            void CancelAction() { // 方法中定义的方法：用来定义如果任务取消，的回调
                if (!self.requestCallbacks.TryGetValue(rpcId, out RpcInfo action)) return;
                self.requestCallbacks.Remove(rpcId); // 去除回调
                Type responseType = OpcodeTypeComponent.Instance.GetResponseType(action.Request.GetType());
                IResponse response = (IResponse) Activator.CreateInstance(responseType);
                response.Error = ErrorCore.ERR_Cancel;
                action.Tcs.SetResult(response); // 设置结果
            }
            IResponse ret; 
            try {
                cancellationToken?.Add(CancelAction); // 设置：取消回调
                ret = await rpcInfo.Tcs; // 等待【异步任务】完成
            } finally {
                cancellationToken?.Remove(CancelAction);
            }
            return ret;
        }
        public static async ETTask<IResponse> Call(this Session self, IRequest request) {
            int rpcId = ++Session.RpcId; // 自增变量：管理体系里的Unique 身份证号
            RpcInfo rpcInfo = new RpcInfo(request);
            self.requestCallbacks[rpcId] = rpcInfo; // 【注册回调】：加入到会话框层面的管理体系里。上面返回消息时，才会对应 rpcId 回调
            request.RpcId = rpcId; // rpcId: 封装得发送消息，会写进回复消息里
            self.Send(request);    // <<<<<<<<<<<<<<<<<<<< 调用上面：这个类里的方法 
            return await rpcInfo.Tcs; // 大致消息发送与回收看一遍；过程细节中仍有不明白的地方。Tcs 这里的功能是一样的。异步任务的完成，就是返回网络异步调用的结果 
        }
        public static void Send(this Session self, IMessage message) { // 【会话框上发消息】：
            self.Send(0, message);
        }
        public static void Send(this Session self, long actorId, IMessage message) {
            self.LastSendTime = TimeHelper.ClientNow();
            OpcodeHelper.LogMsg(self.DomainZone(), message);
            NetServices.Instance.SendMessage(self.ServiceId, self.Id, actorId, message); // 调用网络操作，最底层服务的主线程与网络线程封装来发消息
        }
    }
    [ChildOf]  // 这个例子：就是先前，当多个组件时的处理，不需要具体申明具体的组件类型
    public sealed class Session: Entity, IAwake<int>, IDestroy {
        public int ServiceId { get; set; } // 身份证号
        public static int RpcId {
            get; set;
        }
        // 封装在【会话框】层面，来统一管理各会话框上【发送消息】的各种回调。找一个具体注册回调的例子来看
        public readonly Dictionary<int, RpcInfo> requestCallbacks = new Dictionary<int, RpcInfo>(); // 回调管理
        public long LastRecvTime {
            get;
            set;
        }
        public long LastSendTime {
            get;
            set;
        }
        public int Error {
            get;
            set;
        }
        public IPEndPoint RemoteAddress {
            get;
            set;
        }
    }
}