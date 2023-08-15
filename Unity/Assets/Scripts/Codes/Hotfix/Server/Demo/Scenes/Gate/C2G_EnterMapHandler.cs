using System;
namespace ET.Server {
    // 【网关服】：客户端申请，进入房间。
    [MessageHandler(SceneType.Gate)]
    public class C2G_EnterMapHandler : AMRpcHandler<C2G_EnterMap, G2C_EnterMap> {

		protected override async ETTask Run(Session session, C2G_EnterMap request, G2C_EnterMap response) { 
            Player player = session.GetComponent<SessionPlayerComponent>().GetMyPlayer();
            // 在Gate上动态创建一个Map Scene，把Unit从DB中加载放进来，然后传送到真正的Map中，这样登陆跟传送的逻辑就完全一样了【源】
            // 【在Gate上动态创建一个Map Scene】, 是SceneType.Map, 是，网关服同一进程上再多开一条线程的真正场景，调用的创建场景的方法，创建场景的类型
            // 是创建了一个真正的【SceneType.Map 场景】，并把索引传给了 gateMapComponent.Scene. 没读懂，怎么哪里是从数据库中加载进来的？
            GateMapComponent gateMapComponent = player.AddComponent<GateMapComponent>();
            gateMapComponent.Scene = await SceneFactory.CreateServerScene(gateMapComponent, player.Id, IdGenerater.Instance.GenerateInstanceId(), gateMapComponent.DomainZone(), "GateMap", SceneType.Map);
            Scene scene = gateMapComponent.Scene;
            
            // 这里可以从DB中加载Unit
            Unit unit = UnitFactory.Create(scene, player.Id, UnitType.Player);
            unit.AddComponent<UnitGateComponent, long>(session.InstanceId); 
                
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetBySceneName(session.DomainZone(), "Map1"); // 这个东西，应该也是设置在配置里，存在预设的哪里。【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
            response.MyId = player.Id;

            // 等到一帧的最后面再传送，先让G2C_EnterMap返回，否则传送消息可能比G2C_EnterMap还早
            TransferHelper.TransferAtFrameFinish(unit, startSceneConfig.InstanceId, startSceneConfig.Name).Coroutine();
            await ETTask.CompletedTask; // 这行是我加的，应该是可以适配返回参数，成为ETTask 的！！！
        }
	}
}