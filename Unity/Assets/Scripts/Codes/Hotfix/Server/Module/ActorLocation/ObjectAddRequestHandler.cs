using System;
namespace ET.Server {
    [ActorMessageHandler(SceneType.Location)]
    public class ObjectAddRequestHandler: AMActorRpcHandler<Scene, ObjectAddRequest, ObjectAddResponse> {
        // 狠奇怪：这里为什么不能用ETVoid?
        protected override async void Run(Scene scene, ObjectAddRequest request, ObjectAddResponse response) {
            await scene.GetComponent<LocationComponent>().Add(request.Key, request.InstanceId);
        }
    }
}