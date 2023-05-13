using System;
using System.Net;
using ETModel;
namespace ET.Server {
    // 【没想明白】：服务器在什么情况，前提下，会将玩家踢出房间？有点儿过分，可能用不上
    [MessageHandler(AppType.Gate)]
    public class R2G_PlayerKickOut_ReqHandler : AMRpcHandler<R2G_PlayerKickOut_Req, G2R_PlayerKickOut_Ack> {

        protected override async void Run(Session session, R2G_PlayerKickOut_Req message, Action<G2R_PlayerKickOut_Ack> reply) {
            G2R_PlayerKickOut_Ack response = new G2R_PlayerKickOut_Ack();
            try {
                User user = Game.Scene.GetComponent<UserComponent>().Get(message.UserID);
                // 服务端主动断开客户端连接
                long userSessionId = user.GetComponent<UnitGateComponent>().GateSessionActorId;
                Game.Scene.GetComponent<NetOuterComponent>().Remove(userSessionId);
                Log.Info($"将玩家{message.UserID}连接断开");
                reply(response);
            }
            catch (Exception e) {
                ReplyError(response, e, reply);
            }
        }
    }
}
