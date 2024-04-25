using System;
using Unity.Mathematics;
namespace ET.Server {

	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	// 这个类说：用户，游戏玩家，如果想要纤进程时，用户游戏发请求消息后，地图服的处理、玩家纤进程请求的逻辑
	[ActorMessageHandler(SceneType.Map)]
    public class M2M_UnitTransferRequestHandler : AMActorRpcHandler<Scene, M2M_UnitTransferRequest, M2M_UnitTransferResponse> {

        protected override async ETTask Run(Scene scene, M2M_UnitTransferRequest request, M2M_UnitTransferResponse response) {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit unit = MongoHelper.Deserialize<Unit>(request.Unit);
            unitComponent.AddChild(unit);
            unitComponent.Add(unit);
            foreach (byte[] bytes in request.Entitys) {
                Entity entity = MongoHelper.Deserialize<Entity>(bytes);
                unit.AddComponent(entity);
            }
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<PathfindingComponent, string>(scene.Name);
            unit.Position = new float3(-10, 0, -10);
            unit.AddComponent<MailBoxComponent>(); // 这个时候添加的这个
            // 通知客户端开始切场景
            M2C_StartSceneChange m2CStartSceneChange = new M2C_StartSceneChange() {SceneInstanceId = scene.InstanceId, SceneName = scene.Name};
            MessageHelper.SendToClient(unit, m2CStartSceneChange);
            // 通知客户端创建My Unit
            M2C_CreateMyUnit m2CCreateUnits = new M2C_CreateMyUnit();
            m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
            MessageHelper.SendToClient(unit, m2CCreateUnits);
            // 加入aoi
            unit.AddComponent<AOIEntity, int, float3>(9 * 1000, unit.Position);

            // 解锁location，可以接收发给Unit的消息
			// 【TODO】：找这之前，因切场景、上锁的逻辑
            await LocationProxyComponent.Instance.UnLock(LocationType.Unit, unit.Id, request.OldInstanceId, unit.InstanceId);
        }
    }
}