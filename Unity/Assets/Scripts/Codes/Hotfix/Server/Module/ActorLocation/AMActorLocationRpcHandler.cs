using System;
namespace ET.Server {
    // 【抽象基类】：接口定义错误，务必【异步方法】！！
    [EnableClass] // 接口仍被自己弄得乱七八糟的。。。
    public abstract class AMActorLocationRpcHandler<E, Request, Response>: IMActorHandler where E : Entity where Request : class, IActorLocationRequest where Response : class, IActorLocationResponse {
        // protected abstract ETTask Run(E unit, Request request, Response response);
        protected abstract void Run(E unit, Request request, Response response); // 【异步方法】：【位置服】的异步处理方法，怎么可以改成同步方法的？！！！

        public void Handle(Entity entity, int fromProcess, object actorMessage) {
            try {
                if (actorMessage is not Request request) {
                    Log.Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Request).Name}");
                    return;
                }
                if (entity is not E ee) {
                    Log.Error($"Actor类型转换错误: {entity.GetType().Name} to {typeof (E).Name} --{typeof (Request).Name}");
                    return;
                }
                int rpcId = request.RpcId;
                Response response = Activator.CreateInstance<Response>();
                try {
                    //await this.Run(ee, request, response);
                    this.Run(ee, request, response); // 同样不对。【位置服】处理单线程多进程位置注册、上锁更新、与索要请求等，是队列并发处理，一定是【异步方法】
                }
                catch (Exception exception) {
                    Log.Error(exception);
                    response.Error = ErrorCore.ERR_RpcFail;
                    response.Message = exception.ToString();
                }
                response.RpcId = rpcId; // RpcId
                ActorHandleHelper.Reply(fromProcess, response); // 自动回复，【位置服】返回的【位置回复消息】
            } catch (Exception e) {
                throw new Exception($"解释消息失败: {actorMessage.GetType().FullName}", e);
            }
        }
        public Type GetRequestType() {
            return typeof (Request);
        }
        public Type GetResponseType() {
            return typeof (Response);
        }
	}
}