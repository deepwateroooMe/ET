using System;
namespace ET.Server {
    [ActorMessageHandler(SceneType.Location)] // 标签
    public class ObjectGetRequestHandler: AMActorRpcHandler<Scene, ObjectGetRequest, ObjectGetResponse> {

        protected override async ETTask Run(Scene scene, ObjectGetRequest request, ObjectGetResponse response) {
            // 获取LocationComponent，然后返回一个当前ID对应的最新的instanceId即可
            long instanceId = await scene.GetComponent<LocationComponent>().Get(request.Key);
            response.InstanceId = instanceId;
        }
    }
}