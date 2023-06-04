using System;
namespace ET.Server {
    [ActorMessageHandler(SceneType.Location)]
<<<<<<< HEAD
    public class ObjectUnLockRequestHandler: AMActorRpcHandler<Scene, ObjectUnLockRequest, ObjectUnLockResponse> {
        protected override async void Run(Scene scene, ObjectUnLockRequest request, ObjectUnLockResponse response) {
            scene.GetComponent<LocationComponent>().UnLock(request.Key, request.OldInstanceId, request.InstanceId);
=======
    public class ObjectUnLockRequestHandler: AMActorRpcHandler<Scene, ObjectUnLockRequest, ObjectUnLockResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectUnLockRequest request, ObjectUnLockResponse response)
        {
            scene.GetComponent<LocationManagerComoponent>().Get(request.Type).UnLock(request.Key, request.OldInstanceId, request.InstanceId);

>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
            await ETTask.CompletedTask;
        }
    }
}