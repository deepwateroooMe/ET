using ET;
using System.Net;
using System.Threading.Tasks;

namespace ET.Server {
    public static class RealmHelper {

        // 将玩家踢下线
        public static async Task KickOutPlayer(long userId) {
            // 验证账号是否在线，在线则踢下线
            int gateAppId = Game.Scene.GetComponent<OnlineComponent>().Get(userId);
            if (gateAppId != 0) {
                StartConfig userGateConfig = Game.Scene.GetComponent<StartConfigComponent>().Get(gateAppId);
                IPEndPoint userGateIPEndPoint = userGateConfig.GetComponent<InnerConfig>().IPEndPoint;
                Session userGateSession = Game.Scene.GetComponent<NetInnerComponent>().Get(userGateIPEndPoint);
                await userGateSession.Call(new R2G_PlayerKickOut_Req() { UserID = userId });
                Log.Info($"玩家{userId}已被踢下线");
            }
        }
    }
}
