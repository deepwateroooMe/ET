using System;
using ET;
namespace ET.Server {
    [MessageHandler(SceneType.Map)]
    public class MH2MP_CreateRoom_ReqHandler : AMRpcHandler<MH2MP_CreateRoom_Req, MP2MH_CreateRoom_Ack> {
        protected override async ETTask Run(Session session, MH2MP_CreateRoom_Req message, MP2MH_CreateRoom_Ack response) {
            // 创建房间
            Room room = ComponentFactory.Create<Room>();
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