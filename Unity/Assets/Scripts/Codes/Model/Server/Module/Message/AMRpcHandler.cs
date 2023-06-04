using System;
// protected abstract void Run(Session session, Request request, Response response);
// private async void HandleAsync(Session session, object message) {
namespace ET.Server {
    public abstract class AMRpcHandler<Request, Response>: IMHandler where Request : class, IRequest where Response : class, IResponse {
        protected static void ReplyError(Response response, Exception e, Action<Response> reply) {
            Log.Error(e);
            response.Error = ErrorCode.ERR_RpcFail;
            response.Message = e.ToString();
            reply(response);
        }
        protected abstract void Run(Session session, Request message, Action<Response> reply);
        public void Handle(Session session, object message) {
            try {
                Request request = message as Request;
<<<<<<< HEAD
                if (request == null) 
                    Log.Error($"消息类型转换错误: {message.GetType().Name} to {typeof (Request).Name}");
=======
                if (request == null)
                {
                    throw new Exception($"消息类型转换错误: {message.GetType().FullName} to {typeof (Request).FullName}");
                }

>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
                int rpcId = request.RpcId;
                long instanceId = session.InstanceId;
                this.Run(session, request, response => {
// 等回调回来,session可以已经断开了,所以需要判断session InstanceId是否一样
                    if (session.InstanceId != instanceId)
                        return;
                    response.RpcId = rpcId;
                    session.Send(response);
                });
            }
            catch (Exception e) {
                throw new Exception($"解释消息失败: {message.GetType().FullName}", e);
            }
        }
        public Type GetMessageType() {
            return typeof (Request);
        }
    }
        // 把原类定义备份一下，重新改成参考项目一样的
    //     protected abstract ETTask Run(Session session, Request request, Response response);
    //     public void Handle(Session session, object message) {
    //         HandleAsync(session, message).Coroutine();
    //     }
    //     private async ETTask HandleAsync(Session session, object message) {
    //         try {
    //             Request request = message as Request;
    //             if (request == null) {
    //                 throw new Exception($"消息类型转换错误: {message.GetType().Name} to {typeof (Request).Name}");
    //             }
    //             int rpcId = request.RpcId;
    //             long instanceId = session.InstanceId;
    //             Response response = Activator.CreateInstance<Response>();
    //             try {
    //                 await this.Run(session, request, response);
    //             }
    //             catch (Exception exception) {
    //                 Log.Error(exception);
    //                 response.Error = ErrorCore.ERR_RpcFail;
    //                 response.Message = exception.ToString();
    //             }
    //             // 等回调回来,session可以已经断开了,所以需要判断session InstanceId是否一样
    //             if (session.InstanceId != instanceId) 
    //                 return;
    //             response.RpcId = rpcId; // 在这里设置rpcId是为了防止在Run中不小心修改rpcId字段
    //             session.Send(response);
    //         }
    //         catch (Exception e) {
    //             throw new Exception($"解释消息失败: {message.GetType().FullName}", e);
    //         }
    //     }
    //     public Type GetMessageType() {
    //         return typeof (Request);
    //     }
    //     public Type GetResponseType() {
    //         return typeof (Response);
    //     }
    // }
}





