using System;
namespace ET.Server {
// 抽象基类：框架底层封装过的功能逻辑：跨进程【发送消息】与【返回消息】的 rpcId 完全一致 + 【返回消息】自动回复，两大底层封装
    [EnableClass] 
    public abstract class AMActorRpcHandler<E, Request, Response>: IMActorHandler where E : Entity where Request : class, IActorRequest where Response : class, IActorResponse {
        protected abstract ETTask Run(E unit, Request request, Response response);

        // 抽象基类：封装在框架最底层的逻辑：
            // 任何需要【返回消息】的【请求消息】，请求与返回消息的 actorId 一定一一对应完全一致。这是应该的，所以也封装在跨进程 actor 消息处理器的最底层
            // 自动生成最基本返回消息；异步等待各小服【细节实现】各小服的特异逻辑，执行完成；再，这里框架最底层，继续封装，返回消息的返回回复过程。
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
                    // 实体实现类：里面，可以各自特异的【各小服、分功能，独特处理的细节】异步方法 
                    this.Run(ee, request, response).Coroutine(); // 【特别检查】运行时是否抛错与异常。感觉这里被自己改得。。
                    // await this.Run(ee, request, response);
                }
                catch (Exception exception) {
                    Log.Error(exception);
                    response.Error = ErrorCore.ERR_RpcFail;
                    response.Message = exception.ToString();
                }
                response.RpcId = rpcId; // 封装【发送】与【返回】消息的 rpcId 完全一致
// 自动化包装：【返回消息】的自动回复过程。所有进程间返回消息【回复过程】一致，模块功能逻辑封装在ActorHandleHelper 帮助类里
                ActorHandleHelper.Reply(fromProcess, response); // <<<<<<<<<<<<<<<<<<<< 
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