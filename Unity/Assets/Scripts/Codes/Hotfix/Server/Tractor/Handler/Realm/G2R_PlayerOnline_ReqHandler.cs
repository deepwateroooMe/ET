using ET;
using System;
namespace ET.Server {
    [MessageHandler(SceneType.Realm)]
    public class G2R_PlayerOnline_ReqHandler : AMRpcHandler<G2R_PlayerOnline_Req, R2G_PlayerOnline_Ack> {
        // 【古老版本里的】：可能可以删除的
        protected override async ETTask Run(Session session, G2R_PlayerOnline_Req message, R2G_PlayerOnline_Ack response) {
            OnlineComponent onlineComponent = Root.Instance.Scene.GetComponent<OnlineComponent>();
            // 将已在线玩家踢下线
            await RealmHelper.KickOutPlayer(message.UserID);
            // 玩家上线
            onlineComponent.Add(message.UserID, message.GateAppID);
            Log.Info($"玩家{message.UserID}上线");
        }
    }
}