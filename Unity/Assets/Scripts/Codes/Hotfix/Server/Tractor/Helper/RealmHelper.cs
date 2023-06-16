using ET;
using System.Net;
using System.Threading.Tasks;
namespace ET.Server {
    public static class RealmHelper {
        // 将玩家踢下线
        public static async Task KickOutPlayer(long userId) {
            // 验证账号是否在线，在线则踢下线
            int gateAppId = OnlineComponentSystem.Get(Root.Instance.Scene.GetComponent<OnlineComponent>(), userId);
            if (gateAppId != 0) {
                StartConfig userGateConfig = Root.Instance.Scene.GetComponent<StartConfigComponent>().Get(gateAppId);
                IPEndPoint userGateIPEndPoint = userGateConfig.GetComponent<InnerConfig>().IPEndPoint;
                Session userGateSession = NetInnerComponentSystem.Get(Root.Instance.Scene.GetComponent<NetInnerComponent>(), userGateIPEndPoint);
                await userGateSession.Call(new R2G_PlayerKickOut_Req() { UserID = userId });
                Log.Info($"玩家{userId}已被踢下线");
            }
        }
    }
}