﻿using System;
namespace ET.Server {
    [EnableClass] // 接口仍被自己弄得乱七八糟的。。。
    public abstract class AMActorLocationRpcHandler<E, Request, Response>: IMActorHandler where E : Entity where Request : class, IActorLocationRequest where Response : class, IActorLocationResponse {
        // protected abstract ETTask Run(E unit, Request request, Response response);
        protected abstract void Run(E unit, Request request, Response response);
        public void Handle(Entity entity, int fromProcess, object actorMessage) {
        // public void Handle(Entity entity, int fromProcess, object actorMessage) {
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
                    this.Run(ee, request, response); // 这里是，不对呀。。。
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
            return typeof (Request);
        }
        public Type GetResponseType() {
            return typeof (Response);
        }
	}
}