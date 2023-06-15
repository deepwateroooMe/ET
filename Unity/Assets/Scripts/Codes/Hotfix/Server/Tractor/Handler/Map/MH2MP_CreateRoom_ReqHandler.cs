using System;
using ET;
namespace ET.Server {
    [MessageHandler(SceneType.Map)]
    public class MH2MP_CreateRoom_ReqHandler : AMRpcHandler<MH2MP_CreateRoom_Req, MP2MH_CreateRoom_Ack> {

        protected override async ETTask Run(Session session, MH2MP_CreateRoom_Req message, MP2MH_CreateRoom_Ack response) {
            // 创建房间
            Room room = ComponentFactory.Create<Room>();
            room.AddComponent<DeckComponent>();
            room.AddComponent<DeskCardsCacheComponent>();
            room.AddComponent<OrderControllerComponent>();
            room.AddComponent<GameControllerComponent, RoomConfig>(RoomHelper.GetConfig(RoomLevel.Lv100));
            await room.AddComponent<MailBoxComponent>().AddLocation();
			Root.Instance.Scene.GetComponent<RoomComponent>().Add(room);
            Log.Info($"创建房间{room.InstanceId}");
            response.RoomID = room.InstanceId;
        }
    }
}