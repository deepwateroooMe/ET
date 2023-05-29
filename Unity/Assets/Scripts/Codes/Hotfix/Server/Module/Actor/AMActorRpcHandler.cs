using System;
namespace ET.Server {
    [EnableClass] // 感觉这个类，因为不太理解，被自己改得乱七八糟。明天上午要好好看下，这个被自己改来改去，到底算是怎么回事？【爱表哥，爱生活！！！活宝妹就是一定要嫁给亲爱的表哥！！】
    public abstract class AMActorRpcHandler<E, Request, Response>: IMActorHandler where E : Entity where Request : class, IActorRequest where Response : class, IActorResponse {
        // protected abstract ETTask Run(E unit, Request request, Response response);
        protected abstract void Run(E unit, Request request, Response response);
        // public async ETTask Handle(Entity entity, int fromProcess, object actorMessage) {
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
                    // await this.Run(ee, request, response);
                    this.Run(ee, request, response);
                }
                catch (Exception exception) {
                    Log.Error(exception);
                    response.Error = ErrorCore.ERR_RpcFail;
                    response.Message = exception.ToString();
                }
                response.RpcId = rpcId;
                ActorHandleHelper.Reply(fromProcess, response);
            }
            catch (Exception e) {
                throw new Exception($"解释消息失败: {actorMessage.GetType().FullName}", e);
            }
        }
        public Type GetRequestType() {
            if (typeof (IActorLocationRequest).IsAssignableFrom(typeof (Request)))
                Log.Error($"message is IActorLocationMessage but handler is AMActorRpcHandler: {typeof (Request)}");
            return typeof (Request);
        }
        public Type GetResponseType() {
            return typeof (Response);
        }
    }
}