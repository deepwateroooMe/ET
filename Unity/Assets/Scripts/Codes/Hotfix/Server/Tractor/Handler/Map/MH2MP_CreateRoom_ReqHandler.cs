using System;
using ET;
namespace ET.Server {
    [MessageHandler(SceneType.Map)]
    public class MH2MP_CreateRoom_ReqHandler : AMRpcHandler<MH2MP_CreateRoom_Req, MP2MH_CreateRoom_Ack> {
        protected override async ETTask Run(Session session, MH2MP_CreateRoom_Req message, MP2MH_CreateRoom_Ack response) {
            // 创建房间: 这里现在的问题是，现重构的游戏项目，我还没能进展到这里来，还不曾在任何地方添加过RoomComponent 组件 !! 添加进了根场景下的全服里
            RoomComponent roomComponent = session.DomainScene().GetComponent<RoomComponent>();
            Room room = roomComponent.AddChild<Room, long>(IdGenerater.Instance.GenerateInstanceId()); // 现在说，房间也是门牌号，随机生成一个
            // 这里是先去拿到 RoomComponent.System ？？, 再用它去创建房间。可是这个类现在不完整，项目还没能学习好
            // Room room = 
            room.AddComponent<DeckComponent>();
            room.AddComponent<DeskCardsCacheComponent>();
            room.AddComponent<OrderControllerComponent>();
            room.AddComponent<GameControllerComponent, RoomConfig>(RoomHelper.GetConfig(RoomLevel.Lv100));
            await room.AddComponent<MailBoxComponent>().AddLocation();
			RoomComponentSystem.Add(Root.Instance.Scene.GetComponent<RoomComponent>(), room);
            Log.Info($"创建房间{room.InstanceId}");
            response.RoomID = room.InstanceId;
        }
    }
}