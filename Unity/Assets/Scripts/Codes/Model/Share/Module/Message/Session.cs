using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
namespace ET {
    public readonly struct RpcInfo { // 【消息】包装体：可以是进程内的。可是它包装的是基类接口，与扩展接口如何区分？
        public readonly IRequest Request;
        public readonly ETTask<IResponse> Tcs;
        public RpcInfo(IRequest request) {
            this.Request = request;
            this.Tcs = ETTask<IResponse>.Create(true);
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
        // 【往回去找】：会话框上发IRequest 消息时，是如何注册回调到会话框的管理字典的？
        public static void OnResponse(this Session self, IResponse response) {
            if (!self.requestCallbacks.TryGetValue(response.RpcId, out var action)) // 从【会话框】层的字典取先前注册过的该 RpcId 的回调
                return;
            self.requestCallbacks.Remove(response.RpcId); // 字典管理. 【下面】：如果有异常，写进Tcs 里，框架（异步任务模块）是会自动抛出异常的，不用再多管了
            if (ErrorCore.IsRpcNeedThrowException(response.Error)) {
                action.Tcs.SetException(new Exception($"Rpc error, request: {action.Request} response: {response}"));
                return;
            }
            action.Tcs.SetResult(response); // 写结果 
        }
        public static async ETTask<IResponse> Call(this Session self, IRequest request, ETCancellationToken cancellationToken) {
            int rpcId = ++Session.RpcId;
            RpcInfo rpcInfo = new RpcInfo(request);
            self.requestCallbacks[rpcId] = rpcInfo;
            request.RpcId = rpcId;
            self.Send(request);
            void CancelAction() { // 方法中定义的方法：用来定义如果任务取消，的回调
                if (!self.requestCallbacks.TryGetValue(rpcId, out RpcInfo action)) 
                    return;
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
            }
            finally {
                cancellationToken?.Remove(CancelAction);
            }
            return ret;
        }
        public static async ETTask<IResponse> Call(this Session self, IRequest request) {
            int rpcId = ++Session.RpcId; // 自增变量
            RpcInfo rpcInfo = new RpcInfo(request);
            self.requestCallbacks[rpcId] = rpcInfo; // 【注册回调】：加入到会话框层面的管理体系里。上面返回消息时，才会对应 rpcId 回调
            request.RpcId = rpcId; // rpcId: 
            self.Send(request);    // <<<<<<<<<<<<<<<<<<<< 调用上面：这个类里的方法 
            return await rpcInfo.Tcs;
        }
        public static void Send(this Session self, IMessage message) {
            self.Send(0, message);
        }
        public static void Send(this Session self, long actorId, IMessage message) {
            self.LastSendTime = TimeHelper.ClientNow();
            OpcodeHelper.LogMsg(self.DomainZone(), message);
            NetServices.Instance.SendMessage(self.ServiceId, self.Id, actorId, message);
        }
    }
    [ChildOf]
    public sealed class Session: Entity, IAwake<int>, IDestroy {
        public int ServiceId { get; set; } // 身份证号
        public static int RpcId {
            get;
            set;
        }
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