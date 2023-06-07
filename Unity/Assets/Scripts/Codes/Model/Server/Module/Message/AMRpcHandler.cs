using System;
namespace ET.Server {
    public abstract class AMRpcHandler<Request, Response>: IMHandler where Request : class, IRequest where Response : class, IResponse {
        protected abstract ETTask Run(Session session, Request request, Response response);
// 看一下：这个返回类型void 算怎么回事？如果这个可以运行通，可以用作参照，来修改其它、另一个上一个不带返回消息的抽象类
        public void Handle(Session session, object message) { 
            HandleAsync(session, message).Coroutine();
        }
        private async ETTask HandleAsync(Session session, object message) {
            try {
                Request request = message as Request;
                if (request == null) 
                    throw new Exception($"消息类型转换错误: {message.GetType().Name} to {typeof (Request).Name}");
                int rpcId = request.RpcId;
                long instanceId = session.InstanceId;
                Response response = Activator.CreateInstance<Response>();
                try { // 不懂：下面一句是在干什么，执行对发送来消息的处理，写返回数据？这里调的是抽象异步函数。会在具体的实现类里去实现
                    await this.Run(session, request, response);
                }
                catch (Exception exception) { // 如果出异常：写异常结果
                    Log.Error(exception);
                    response.Error = ErrorCore.ERR_RpcFail;
                    response.Message = exception.ToString();
                }
                // 等回调回来,session可以已经断开了,所以需要判断session InstanceId是否一样
                if (session.InstanceId != instanceId) 
                    return;
                response.RpcId = rpcId; // 在这里设置rpcId是为了防止在Run中不小心修改rpcId字段。【谁发来的消息，就返回消息给谁——发送者】
                session.Send(response); // 把返回消息发回去
            }
            catch (Exception e) { // 捕获异步操作过程中的异常
                throw new Exception($"解释消息失败: {message.GetType().FullName}", e);
            }
        }
        public Type GetMessageType() {
            return typeof (Request);
        }
        public Type GetResponseType() {
            return typeof (Response);
        }
    }    
}
// 下面的，是参考其它版本来的，不一定正确
// using System;
// // protected abstract void Run(Session session, Request request, Response response);
// // private async void HandleAsync(Session session, object message) {
// namespace ET.Server {
//     public abstract class AMRpcHandler<Request, Response>: IMHandler where Request : class, IRequest where Response : class, IResponse {
//         protected static void ReplyError(Response response, Exception e, Action<Response> reply) {
//             Log.Error(e);
//             response.Error = ErrorCode.ERR_RpcFail;
//             response.Message = e.ToString();
//             reply(response);
//         }
//         protected abstract void Run(Session session, Request message, Action<Response> reply);
//         public void Handle(Session session, object message) {
//             try {
//                 Request request = message as Request;
//                 if (request == null) 
//                     Log.Error($"消息类型转换错误: {message.GetType().Name} to {typeof (Request).Name}");
//                 int rpcId = request.RpcId;
//                 long instanceId = session.InstanceId;
//                 this.Run(session, request, response => {
// // 等回调回来,session可以已经断开了,所以需要判断session InstanceId是否一样
//                     if (session.InstanceId != instanceId)
//                         return;
//                     response.RpcId = rpcId;
//                     session.Send(response);
//                 });
//             }
//             catch (Exception e) {
//                 throw new Exception($"解释消息失败: {message.GetType().FullName}", e);
//             }
//         }
//         public Type GetMessageType() {
//             return typeof (Request);
//         }
//     }
// }
