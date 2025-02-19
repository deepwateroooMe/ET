﻿using System;
using ET;
namespace ET.Server {
    [MessageHandler(SceneType.Match)]
    [FriendOfAttribute(typeof(ET.Server.MatchComponent))]
    public class MP2MH_PlayerExitRoom_ReqHandler : AMRpcHandler<MP2MH_PlayerExitRoom_Req, MH2MP_PlayerExitRoom_Ack> {
        protected override async ETTask Run(Session session, MP2MH_PlayerExitRoom_Req request, MH2MP_PlayerExitRoom_Ack response) {
            MatchRoomComponent matchRoomComponent = Root.Instance.Scene.GetComponent<MatchRoomComponent>();
            Room room = matchRoomComponent.Get(request.RoomID);
            // 移除玩家对象
            Gamer gamer = room.Remove(request.UserID);
            Root.Instance.Scene.GetComponent<MatchComponent>().Playing.Remove(gamer.UserID);
            gamer.Dispose();
            Log.Info($"Match：同步玩家{request.UserID}退出房间");
            if (room.Count == 0) {
                // 当房间中没有玩家时回收
                matchRoomComponent.Recycle(room.Id);
                Log.Info($"回收房间{room.Id}");
            }
            await ETTask.CompletedTask;
        }
    }
}