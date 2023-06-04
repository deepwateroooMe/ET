using System;
namespace ET.Server {
    [ActorMessageHandler(SceneType.Location)]
<<<<<<< HEAD
    public class ObjectRemoveRequestHandler: AMActorRpcHandler<Scene, ObjectRemoveRequest, ObjectRemoveResponse> {
        // protected override async ETTask Run(Scene scene, ObjectRemoveRequest request, ObjectRemoveResponse response) {
        protected override async void Run(Scene scene, ObjectRemoveRequest request, ObjectRemoveResponse response) {
            await scene.GetComponent<LocationComponent>().Remove(request.Key);
=======
    public class ObjectRemoveRequestHandler: AMActorRpcHandler<Scene, ObjectRemoveRequest, ObjectRemoveResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectRemoveRequest request, ObjectRemoveResponse response)
        {
            await scene.GetComponent<LocationManagerComoponent>().Get(request.Type).Remove(request.Key);
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
        }
    }
}