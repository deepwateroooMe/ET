using ET;
using ET.Server;
using System;
namespace ET.Server {

    // 可能狠多类，这里已经可以不要了，因为组件化的自动完成，比如用户下线的会话框自动移除等
    [MessageHandler(SceneType.Realm)]
    public class G2R_PlayerOffline_ReqHandler : AMRpcHandler<G2R_PlayerOffline_Req, R2G_PlayerOffline_Ack> {

        protected override void Run(Session session, G2R_PlayerOffline_Req message,Action<R2G_PlayerOffline_Ack> reply) {
            R2G_PlayerOffline_Ack response = new R2G_PlayerOffline_Ack();
            try {
                // 玩家下线
                Game.Scene.GetComponent<OnlineComponent>().Remove(message.UserID);
                Log.Info($"玩家{message.UserID}下线");
                reply(response);
            }
            catch (Exception e) {
                ReplyError(response, e, reply);
            }
        }
    }
}