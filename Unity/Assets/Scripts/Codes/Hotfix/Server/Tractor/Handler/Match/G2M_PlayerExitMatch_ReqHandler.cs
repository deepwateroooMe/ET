using System;
using ET;
namespace ET.Server {

    [MessageHandler(SceneType.Match)]
    public class G2M_PlayerExitMatch_ReqHandler : AMRpcHandler<G2M_PlayerExitMatch_Req, M2G_PlayerExitMatch_Ack> {

        protected override ETTask Run(Session session, G2M_PlayerExitMatch_Req message, M2G_PlayerExitMatch_Ack response) {
            Matcher matcher = await Root.Instance.Scene.GetComponent<MatcherComponent>().Remove(message.UserID);
            matcher?.Dispose();
            Log.Info($"玩家{message.UserID}退出匹配队列");
        }
    }
}
