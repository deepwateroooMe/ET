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
                Response response = Activator.CreateInstance<Response>(); // 创建一个消息的回复实例
                try { // 【找个例子看一下】不懂：下面一句是在干什么，执行对发送来消息的处理，写返回数据？这里调的是抽象异步函数。会在具体的实现类里去实现
                    await this.Run(session, request, response); // 猜测：应该更多可能是，通过不同服的具体实现，将返回数据写好？因为发送还在后面
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
                session.Send(response); // 把返回消息发回去，这里才是真正的发返回消息回请求端
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








