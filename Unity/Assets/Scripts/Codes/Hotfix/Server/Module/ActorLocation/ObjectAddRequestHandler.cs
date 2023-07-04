using System;
namespace ET.Server {

    [ActorMessageHandler(SceneType.Location)]
    public class ObjectAddRequestHandler: AMActorRpcHandler<Scene, ObjectAddRequest, ObjectAddResponse> {
        protected override async ETTask Run(Scene scene, ObjectAddRequest request, ObjectAddResponse response) {
            // 是【服务端】的根场景——位置服务器场景中，添加过这个位置组件. 需要拿到位置服务器的场景
            // 下面的热更域里的实现类：可能没有定义 LocationComponentSystem, 要补上
            await LocationComponentSystem.Add(Root.Instance.Scene.GetComponent<LocationComponent>(), request.Key, request.InstanceId);
        }
    }
}
// 【任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！活宝妹若是还没能嫁给亲爱的表哥，活宝妹就永远守候在亲爱的表哥的身边！！爱表哥，爱生活！！！】