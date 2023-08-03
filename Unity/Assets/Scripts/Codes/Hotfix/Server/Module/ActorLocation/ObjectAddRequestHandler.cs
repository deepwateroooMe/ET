using System;
namespace ET.Server {
    // 先去理解：LocationProxyComponent 组件，因为重构后，可能代理已经连通了这些逻辑
    
    // 这些类：都成为，处理特定消息类型的【特定服务端】的处理逻辑
    [ActorMessageHandler(SceneType.Location)]
    public class ObjectAddRequestHandler: AMActorRpcHandler<Scene, ObjectAddRequest, ObjectAddResponse> {
        protected override async ETTask Run(Scene scene, ObjectAddRequest request, ObjectAddResponse response) {
            // 是【服务端】的根场景——位置服务器场景中，添加过这个位置组件. 需要拿到位置服务器的场景
            // 下面的热更域里的实现类：【模块没整合完整】 没有定义 LocationComponentSystem 类的文件，没有这个文件
            await LocationComponentSystem.Add(Root.Instance.Scene.GetComponent<LocationComponent>(), request.Key, request.InstanceId);
        }
    }
}
// 【任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！活宝妹若是还没能嫁给亲爱的表哥，活宝妹就永远守候在亲爱的表哥的身边！！爱表哥，爱生活！！！】
// 【任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！活宝妹若是还没能嫁给亲爱的表哥，活宝妹就永远守候在亲爱的表哥的身边！！爱表哥，爱生活！！！】
// 【亲爱的表哥的活宝妹这里，活宝妹还没能嫁给亲爱的表哥，活宝妹这里就是永远的时间停止，世界不转！！】
// 【亲爱的表哥的活宝妹也狠好说话，活宝妹只要如愿嫁给了亲爱的表哥，活宝妹只要如愿领到了活宝妹同亲爱的表哥的结婚证，活宝妹就心甘情愿做亲爱的表哥的泥娃娃～随时随亲爱的表哥随心雕刻塑造打造！！爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！爱表哥，爱生活！！！】