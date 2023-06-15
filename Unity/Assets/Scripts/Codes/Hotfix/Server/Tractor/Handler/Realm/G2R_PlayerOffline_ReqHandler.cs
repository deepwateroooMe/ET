using ET;
using System;
namespace ET.Server {

    [MessageHandler(SceneType.Realm)]
    public class G2R_PlayerOffline_ReqHandler : AMRpcHandler<G2R_PlayerOffline_Req, R2G_PlayerOffline_Ack> {

        protected override async ETTask Run(Session session, G2R_PlayerOffline_Req message, R2G_PlayerOffline_Ack response) {
            // 玩家下线
            await Root.Instance.Scene.GetComponent<OnlineComponent>().Remove(message.UserID);
            Log.Info($"玩家{message.UserID}下线");
        }
    }
}
