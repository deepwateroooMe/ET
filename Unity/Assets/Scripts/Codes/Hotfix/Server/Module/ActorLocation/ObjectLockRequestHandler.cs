using System;
namespace ET.Server {
    [ActorMessageHandler(SceneType.Location)]
<<<<<<< HEAD
    public class ObjectLockRequestHandler: AMActorRpcHandler<Scene, ObjectLockRequest, ObjectLockResponse> {
        // protected override async ETTask Run(Scene scene, ObjectLockRequest request, ObjectLockResponse response) {
        protected override async void Run(Scene scene, ObjectLockRequest request, ObjectLockResponse response) {
            await scene.GetComponent<LocationComponent>().Lock(request.Key, request.InstanceId, request.Time);
=======
    public class ObjectLockRequestHandler: AMActorRpcHandler<Scene, ObjectLockRequest, ObjectLockResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectLockRequest request, ObjectLockResponse response)
        {
            await scene.GetComponent<LocationManagerComoponent>().Get(request.Type).Lock(request.Key, request.InstanceId, request.Time);
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
        }
    }
}