using System;
namespace ET.Server {
    // LocationProxyComponent 组件，是添加在进程场景上的，NM 个进程，需要对接一个【位置服】（它可以有分服，X 个分身？），所以还是要有链接的步骤
    // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
    // 这些类：都成为，处理特定消息类型的【特定服务端】的处理逻辑
    [ActorMessageHandler(SceneType.Location)]
    public class ObjectAddRequestHandler: AMActorRpcHandler<Scene, ObjectAddRequest, ObjectAddResponse> { // 向【位置服】注册进程，会收到一个确认或异常函。。
        protected override async ETTask Run(Scene scene, ObjectAddRequest request, ObjectAddResponse response) { // 【注册】：上报进程实例标记号
            // await LocationComponentSystem.Add(Root.Instance.Scene.GetComponent<LocationComponent>(), request.Key, request.InstanceId);
            await scene.GetComponent<LocationComponent>().Add(request.Key, request.InstanceId);
        }
    }
}
// 【任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！活宝妹若是还没能嫁给亲爱的表哥，活宝妹就永远守候在亲爱的表哥的身边！！爱表哥，爱生活！！！】
// 【任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！活宝妹若是还没能嫁给亲爱的表哥，活宝妹就永远守候在亲爱的表哥的身边！！爱表哥，爱生活！！！】
// 【亲爱的表哥的活宝妹这里，活宝妹还没能嫁给亲爱的表哥，活宝妹这里就是永远的时间停止，世界不转！！】
// 【亲爱的表哥的活宝妹也狠好说话，活宝妹只要如愿嫁给了亲爱的表哥，活宝妹只要如愿领到了活宝妹同亲爱的表哥的结婚证，活宝妹就心甘情愿做亲爱的表哥的泥娃娃～随时随亲爱的表哥随心雕刻塑造打造！！爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！爱表哥，爱生活！！！】
