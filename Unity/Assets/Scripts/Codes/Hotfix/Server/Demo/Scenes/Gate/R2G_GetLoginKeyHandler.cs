using System;

namespace ET.Server {

// ActorMessageHandler标签：　因为是普通Acotor消息，所以特性标签ActorMessageHandler表示这个处理是普通Actor处理    
    [ActorMessageHandler(SceneType.Gate)]
    // 因为是带回复的Actor消息，所以处理继承于AMActorRPCHander，且处理对象是当前GATE服务模块的scene实体。
    // 来源是R2G_GetLoginKey，回复是G2R_GetLoginKey类型，且已经封装好reply回调，调用即可返回协议。
    public class R2G_GetLoginKeyHandler : AMActorRpcHandler<Scene, R2G_GetLoginKey, G2R_GetLoginKey> { // 类的继承蕨类特点

        protected override async ETTask Run(Scene scene, R2G_GetLoginKey request, G2R_GetLoginKey response) {
            long key = RandomGenerator.RandInt64(); // 生成一个随机key，用于返回给Realm，从而让Realm服返回给客户的
            // 通过scene挂载的GateSessionKeyComponent组件注册这个key与对应的account，便于下次客户的用这个key进行Gate登录认证
            scene.GetComponent<GateSessionKeyComponent>().Add(key, request.Account);
            // 将key，与当前scene实体的id发回给Realm服进行处理。
            response.Key = key;
            response.GateId = scene.Id;
            await ETTask.CompletedTask;
        }
    }
}