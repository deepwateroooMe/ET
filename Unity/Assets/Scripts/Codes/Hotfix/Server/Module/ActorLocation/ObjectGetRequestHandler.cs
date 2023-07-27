using System;
namespace ET.Server {
// 【位置服】：处理索拿小伙伴信息逻辑。这里的逻辑不完整，应该是我从参考项目【斗地主游戏】或是【卡星麻将】里参考过来的。【服务端】逻辑不完整，就不看了
    [ActorMessageHandler(SceneType.Location)] 
    public class ObjectGetRequestHandler: AMActorRpcHandler<Scene, ObjectGetRequest, ObjectGetResponse> {

        protected override async ETTask Run(Scene scene, ObjectGetRequest request, ObjectGetResponse response) {
            long instanceId = await scene.GetComponent<LocationComponent>().Get(request.Key);
            response.InstanceId = instanceId;
        }
    }
}