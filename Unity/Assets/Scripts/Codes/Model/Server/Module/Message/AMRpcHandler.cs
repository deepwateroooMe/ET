using System;
namespace ET.Server {
    public abstract class AMRpcHandler<Request, Response>: IMHandler where Request : class, IRequest where Response : class, IResponse {
// ET7 框架里原本的：
        protected abstract ETTask Run(Session session, Request request, Response response);
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
                try { // 执行对发送来消息的，实现了此抽象基类的【各小服、独特、特民逻辑】的处理，写好返回消息的结果
                    await this.Run(session, request, response); // 猜测：应该更多可能是，通过不同服的具体实现，将返回数据写好？是这样的呀
                }
                catch (Exception exception) { // 如果出异常：写异常结果。这里也会自动抛出异常
                    Log.Error(exception);
                    response.Error = ErrorCore.ERR_RpcFail;
                    response.Message = exception.ToString();
                }
                // 等回调回来,session可以已经断开了,所以需要判断session InstanceId是否一样
                if (session.InstanceId != instanceId) 
                    return;
                response.RpcId = rpcId; // 在这里设置rpcId是为了防止在Run中不小心修改rpcId字段。（前面还像是源者写的）
                session.Send(response); 
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