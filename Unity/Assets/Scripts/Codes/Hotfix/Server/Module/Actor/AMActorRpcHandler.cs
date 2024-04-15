using System;
namespace ET.Server {
    [EnableClass] // 【TODO】：查这个标签，理解流程。【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	// 也就是底层的一层封装：封装了【跨进程消息的】自动回复
    public abstract class AMActorRpcHandler<E, Request, Response>: IMActorHandler where E : Entity where Request : class, IActorRequest where Response : class, IActorResponse {
        protected abstract ETTask Run(E unit, Request request, Response response);
        public async ETTask Handle(Entity entity, int fromProcess, object actorMessage) {
            try {
                if (actorMessage is not Request request) {
                    Log.Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Request).Name}");
                    return;
                }
                if (entity is not E ee) {
                    Log.Error($"Actor类型转换错误: {entity.GetType().FullName} to {typeof (E).FullName} --{typeof (Request).FullName}");
                    return;
                }
                int rpcId = request.RpcId;
                Response response = Activator.CreateInstance<Response>();
                
                try { // 下面，就是实体类【服务端、各司其职的各小服】它们的具体处理逻辑
                    await this.Run(ee, request, response);
                }
                catch (Exception exception) {
                    Log.Error(exception);
                    response.Error = ErrorCore.ERR_RpcFail;
                    response.Message = exception.ToString();
                }
                
                response.RpcId = rpcId; // 往返消息的、 rpcId 是一样的，自动封装
                ActorHandleHelper.Reply(fromProcess, response); // 封装自动回复
            }
            catch (Exception e) {
                throw new Exception($"解释消息失败: {actorMessage.GetType().FullName}", e);
            }
        }
        public Type GetRequestType() {
            if (typeof (IActorLocationRequest).IsAssignableFrom(typeof (Request))) {
                Log.Error($"message is IActorLocationMessage but handler is AMActorRpcHandler: {typeof (Request)}");
            }
            return typeof (Request);
        }
        public Type GetResponseType() {
            return typeof (Response);
        }
    }
}