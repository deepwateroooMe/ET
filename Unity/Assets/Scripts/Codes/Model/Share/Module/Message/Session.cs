using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
namespace ET {
	// 会话框上、带【返回消息、诉求】的 rpc 消息、包装体
	// 【会话框】层次上的RpcInfo精致包装：【诉求消息】与【返回消息】一一对应；借助 RpcInfo 里对ETTask 异步任务的封装，来实现会话框上根据 rpcId 来简化，网络异步调用、异步结果返回等
    public readonly struct RpcInfo {
        public readonly IRequest Request;
        public readonly ETTask<IResponse> Tcs; // 包装进：用来写返回消息结果IResponse的异步任务，精华
        public RpcInfo(IRequest request) {
            this.Request = request;
            this.Tcs = ETTask<IResponse>.Create(true); // 实例实体
        }
    }
	[FriendOf(typeof(Session))]
    public static class SessionSystem {
        [ObjectSystem]
        public class SessionAwakeSystem: AwakeSystem<Session, int> {
            protected override void Awake(Session self, int serviceId) {
                self.ServiceId = serviceId;
                long timeNow = TimeHelper.ClientNow();
                self.LastRecvTime = timeNow;
                self.LastSendTime = timeNow;
                self.requestCallbacks.Clear();
                Log.Info($"session create: zone: {self.DomainZone()} id: {self.Id} {timeNow} ");
            }
        }
        [ObjectSystem]
        public class SessionDestroySystem: DestroySystem<Session> {
            protected override void Destroy(Session self) {
                NetServices.Instance.RemoveChannel(self.ServiceId, self.Id, self.Error);
                foreach (RpcInfo responseCallback in self.requestCallbacks.Values.ToArray()) {
                    responseCallback.Tcs.SetException(new RpcException(self.Error, $"session dispose: {self.Id} {self.RemoteAddress}"));
                }
                Log.Info($"session dispose: {self.RemoteAddress} id: {self.Id} ErrorCode: {self.Error}, please see ErrorCode.cs! {TimeHelper.ClientNow()}");
                self.requestCallbacks.Clear();
            }
        }
        public static void OnResponse(this Session self, IResponse response) {
            if (!self.requestCallbacks.TryGetValue(response.RpcId, out var action)) {
                return;
            }
            self.requestCallbacks.Remove(response.RpcId); // 处理返回消息逻辑，删除字典里的回调，立即处理了要
            if (ErrorCore.IsRpcNeedThrowException(response.Error)) { // 过程中异常
                action.Tcs.SetException(new Exception($"Rpc error, request: {action.Request} response: {response}"));
                return;
            }
// 早前发送时候的Call() RpcInfo.Tcs 写结果
            action.Tcs.SetResult(response); // 去找，IRequest 的消息的发送过程
        }
        public static async ETTask<IResponse> Call(this Session self, IRequest request, ETCancellationToken cancellationToken) {
            int rpcId = ++Session.RpcId; // 随便整个自增变量：为什么可以随便弄一个？【会话框】层面自己的、极简管理：只是用来一一对应【诉求消息】与其对应的【返回消息】的 rpcId
            RpcInfo rpcInfo = new RpcInfo(request); // 包装结构体
            self.requestCallbacks[rpcId] = rpcInfo; // 注册： rpcId 诉求消息的、包装体
            request.RpcId = rpcId; // 任何诉求消息：都添加 rpcId, 只为方便跨进程消息
            self.Send(request); // 把消息发送出去， rpcId=0
            void CancelAction() { // 内部方法：取消的回调
                if (!self.requestCallbacks.TryGetValue(rpcId, out RpcInfo action)) {
                    return;
                }
                self.requestCallbacks.Remove(rpcId);
                Type responseType = OpcodeTypeComponent.Instance.GetResponseType(action.Request.GetType());
                IResponse response = (IResponse) Activator.CreateInstance(responseType); // 封装：写最简【返回消息】相关条款
                response.Error = ErrorCore.ERR_Cancel; // 标记：取消
                action.Tcs.SetResult(response); // 写结果，会抛出异常
            }
            IResponse ret;
            try {
                cancellationToken?.Add(CancelAction); // 必要时，注册：取消的回调
                ret = await rpcInfo.Tcs; // 等结果。 await 使上面的OnResponse() 里Tcs.SetResult(response) 后，结果就可以返回这里 ret
            }
            finally {
                cancellationToken?.Remove(CancelAction);
            }
            return ret; // 返回结果
        }
        public static async ETTask<IResponse> Call(this Session self, IRequest request) {
            int rpcId = ++Session.RpcId;
            RpcInfo rpcInfo = new RpcInfo(request);
            self.requestCallbacks[rpcId] = rpcInfo;
            request.RpcId = rpcId;
            self.Send(request);
            return await rpcInfo.Tcs;
        }
        public static void Send(this Session self, IMessage message) {
            self.Send(0, message); 
        }
		// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
        public static void Send(this Session self, long actorId, IMessage message) {
            self.LastSendTime = TimeHelper.ClientNow(); // 最后发送时间
            OpcodeHelper.LogMsg(self.DomainZone(), message);
// 过程：主线程，包装派给异步线程；异步线程通过特定服务实例、向下走【信道、管道】内存流上发、序列化后消息的过程
            NetServices.Instance.SendMessage(self.ServiceId, self.Id, actorId, message); // 封装成网络异步线程任务，异步线程调用相应Service 上发消息
        }
    }
    [ChildOf]
    public sealed class Session: Entity, IAwake<int>, IDestroy {
        public int ServiceId { get; set; }
        public static int RpcId {
            get;
            set;
        }
		// 会话框：是按照 rpcId 为键，来管理包装体
        public readonly Dictionary<int, RpcInfo> requestCallbacks = new Dictionary<int, RpcInfo>();
		// 会话框：收发消息的、最后活动时间；长时间不活动的、会被回收释放系统资源
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