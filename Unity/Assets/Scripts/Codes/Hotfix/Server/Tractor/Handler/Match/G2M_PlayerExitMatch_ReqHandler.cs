using System;
using ET;
namespace ET.Server {
    [MessageHandler(SceneType.Match)]
    public class G2M_PlayerExitMatch_ReqHandler : AMRpcHandler<G2M_PlayerExitMatch_Req, M2G_PlayerExitMatch_Ack> {
        protected override async ETTask Run(Session session, G2M_PlayerExitMatch_Req message, M2G_PlayerExitMatch_Ack response) {
            Matcher matcher = MatcherComponentSystem.Remove(Root.Instance.Scene.GetComponent<MatcherComponent>(), message.UserID);
            matcher?.Dispose();
            Log.Info($"玩家{message.UserID}退出匹配队列");
            await ETTask.CompletedTask;
        }
    }
}