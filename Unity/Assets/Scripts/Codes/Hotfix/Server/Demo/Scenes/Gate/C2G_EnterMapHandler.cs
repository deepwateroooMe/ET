using System;
namespace ET.Server {
    // 【网关服】：客户端申请，进入房间。 什么时候代码变成这个样子的？自己改得一点儿也不记得？别人原始的，我应该也不至于去改它呀？【活宝妹就是一定要嫁给亲爱的表哥！！！】
    [MessageHandler(SceneType.Gate)]
    public class C2G_EnterMapHandler : AMRpcHandler<C2G_EnterMap, G2C_EnterMap> {
		protected override async void Run(Session session, C2G_EnterMap message, Action<G2C_EnterMap> reply) { 
// 方法变成回调的形式：就是，先自己创建一个回复消息的实例，再用传参进来的回调函数把返回消息回复回去：【活宝妹就是一定要嫁给亲爱的表哥！！！】
            G2C_EnterMap response = new G2C_EnterMap(); 
            try {
                // Player player = session.GetComponent<SessionPlayerComponent>().Player;
                Player player = session.GetComponent<SessionPlayerComponent>().GetMyPlayer();
                // 在Gate上动态创建一个Map Scene，把Unit从DB中加载放进来，然后传送到真正的Map中，这样登陆跟传送的逻辑就完全一样了
                GateMapComponent gateMapComponent = player.AddComponent<GateMapComponent>();
                gateMapComponent.Scene = await SceneFactory.CreateServerScene(gateMapComponent, player.Id, IdGenerater.Instance.GenerateInstanceId(), gateMapComponent.DomainZone(), "GateMap", SceneType.Map);
                Scene scene = gateMapComponent.Scene;
            
                // 这里可以从DB中加载Unit
                Unit unit = UnitFactory.Create(scene, player.Id, UnitType.Player);
            
                StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetBySceneName(session.DomainZone(), "Map1");
                response.MyId = player.Id;
                // 等到一帧的最后面再传送，先让G2C_EnterMap返回，否则传送消息可能比G2C_EnterMap还早
                reply(response);
                TransferHelper.TransferAtFrameFinish(unit, startSceneConfig.InstanceId, startSceneConfig.Name).Coroutine();
                reply(response);
            }
            catch (Exception e) {
                ReplyError(response, e, reply);
            }
        }
    }
}