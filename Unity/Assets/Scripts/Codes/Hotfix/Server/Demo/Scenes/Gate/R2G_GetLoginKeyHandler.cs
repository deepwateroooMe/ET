using System;
namespace ET.Server {
	// Actor消息处理器场景：用这个类作例子，大致理解一下， Actor消息处理器、服务端场景的、处理逻辑大致流程
	[ActorMessageHandler(SceneType.Gate)]
    public class R2G_GetLoginKeyHandler : AMActorRpcHandler<Scene, R2G_GetLoginKey, G2R_GetLoginKey> {
        protected override async ETTask Run(Scene scene, R2G_GetLoginKey request, G2R_GetLoginKey response) {
            long key = RandomGenerator.RandInt64();
            scene.GetComponent<GateSessionKeyComponent>().Add(key, request.Account);
            response.Key = key;
            response.GateId = scene.Id;
            await ETTask.CompletedTask;
        }
    }
}