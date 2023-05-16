using ET;
using System;
namespace ET.Server {
    [MessageHandler(SceneType.Realm)]
    public class G2R_PlayerOnline_ReqHandler : AMRpcHandler<G2R_PlayerOnline_Req, R2G_PlayerOnline_Ack> {
        // 【古老版本里的】：可能可以删除的
        protected override async void Run(Session session, G2R_PlayerOnline_Req message,Action<R2G_PlayerOnline_Ack> reply) {
            R2G_PlayerOnline_Ack response = new R2G_PlayerOnline_Ack();
            try {
                OnlineComponent onlineComponent = Game.Scene.GetComponent<OnlineComponent>();
                // 将已在线玩家踢下线
                await RealmHelper.KickOutPlayer(message.UserID);
                // 玩家上线
                onlineComponent.Add(message.UserID, message.GateAppID);
                Log.Info($"玩家{message.UserID}上线");
                reply(response);
            }
            catch (Exception e) {
                ReplyError(response, e, reply);
            }
        }
    }
}
