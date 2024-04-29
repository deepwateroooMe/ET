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
            unitComponent.Add(unit); // 【生成系】里这里是空函数。但是关系不大
            foreach (byte[] bytes in request.Entitys) {
                Entity entity = MongoHelper.Deserialize<Entity>(bytes);
                unit.AddComponent(entity);
            }
            unit.AddComponent<MoveComponent>();
            unit.AddComponent<PathfindingComponent, string>(scene.Name);
            unit.Position = new float3(-10, 0, -10);
            unit.AddComponent<MailBoxComponent>(); // 为玩家添加了【邮箱】，玩家就可以收发消息

            // 通知客户端开始切场景
            M2C_StartSceneChange m2CStartSceneChange = new M2C_StartSceneChange() {SceneInstanceId = scene.InstanceId, SceneName = scene.Name};
            MessageHelper.SendToClient(unit, m2CStartSceneChange); // <<<<<<<<<<<<<<<<<<<< 【客户端】逻辑：客户端收到地图服的命令，就跑去准备切场景；可是它要等地图服把Unit 创建好
            // 通知客户端创建My Unit
            M2C_CreateMyUnit m2CCreateUnits = new M2C_CreateMyUnit();
            m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
			// 【地图服】发消息给【客户端】：Unit 已经创建好了。刚才上面，【客户端】在等的Unit 好了，客户端的协程，可以往下走了
            MessageHelper.SendToClient(unit, m2CCreateUnits);
            // 加入aoi: 游戏大地图中的视野相关组件
            unit.AddComponent<AOIEntity, int, float3>(9 * 1000, unit.Position);

            // 解锁location，可以接收发给Unit的消息【源】：前面 TransferHelper 的类里，发【玩家想要纤进程进地图服】前，先给玩家上过锁，这里纤完解锁
            await LocationProxyComponent.Instance.UnLock(LocationType.Unit, unit.Id, request.OldInstanceId, unit.InstanceId);
        }
    }
} // 亲爱的表哥的活宝妹，想要看的关键逻辑，全都看懂了；不想看、不喜欢、没兴趣的模块、可以不用看！
 // 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】