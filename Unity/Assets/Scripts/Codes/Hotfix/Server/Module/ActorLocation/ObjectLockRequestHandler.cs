using System;
namespace ET.Server {
    [ActorMessageHandler(SceneType.Location)] // 【位置服】Actor 消息处理器、场景
    public class ObjectLockRequestHandler: AMActorRpcHandler<Scene, ObjectLockRequest, ObjectLockResponse> {

        protected override async ETTask Run(Scene scene, ObjectLockRequest request, ObjectLockResponse response) {
			// 把这个看懂：
            await scene.GetComponent<LocationManagerComoponent>().Get(request.Type).Lock(request.Key, request.InstanceId, request.Time);
        }
    }
}