﻿using System;
using System.Threading.Tasks;
using ET;
namespace ET.Server {

    [ActorMessageHandler(SceneType.Map)]
    public class Actor_PlayerExitRoom_ReqHandler : AMActorRpcHandler<Gamer, Actor_PlayerExitRoom_Req, Actor_PlayerExitRoom_Ack> {

        // protected override async Task Run(Gamer gamer, Actor_PlayerExitRoom_Req message, Action<Actor_PlayerExitRoom_Ack> reply) {
        protected override async ETTask Run(Gamer gamer, Actor_PlayerExitRoom_Req message, Actor_PlayerExitRoom_Ack response) {
            // Room room = await Root.Instance.Scene.GetComponent<RoomComponent>().Get(gamer.RoomID);
            Room room = RoomComponentSystem.Get(Root.Instance.Scene.GetComponent<RoomComponent>(), gamer.RoomID);
            if (room.State == RoomState.Game) {
                gamer.isOffline = true;
                // 玩家断开添加自动出牌组件
                if (gamer.GetComponent<TrusteeshipComponent>() == null)
                    gamer.AddComponent<TrusteeshipComponent>();
                Log.Info($"玩家{message.UserID}断开，切换为自动模式");
            } else {
                // 房间移除玩家
                room.Remove(gamer.UserID);
                // 同步匹配服务器移除玩家
                await MapHelper.GetMapSession().Call(new MP2MH_PlayerExitRoom_Req() { RoomID = room.InstanceId, UserID = gamer.UserID });
                // 消息广播给其他人
                room.Broadcast(new Actor_GamerExitRoom_Ntt() { UserID = gamer.UserID });
                Log.Info($"Map：玩家{gamer.UserID}退出房间");
                gamer.Dispose();
            }
        }
    }
}
