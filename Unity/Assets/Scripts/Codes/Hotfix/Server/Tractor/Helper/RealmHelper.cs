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
                // StartConfig userGateConfig = Root.Instance.Scene.GetComponent<StartConfigComponent>().Get(gateAppId);
                // IPEndPoint userGateIPEndPoint = userGateConfig.GetComponent<InnerConfig>().IPEndPoint;
                // StartSceneConfig userGateConfig = StartSceneConfigCategory.Instance.Get(gateAppId); // 这里，先通过场景单例类，拿到对应【网关服】场景
                // IPEndPoint userGateIPEndPoint = userGateConfig.InnerIPOutPort; // 再去拿什么地址。这些还没能够理解透彻，但是先把【编译器】报错给骗过去。。。

                // 下面的：现在，知道，自己先前写得有多敝脚了吧。。。亲爱的表哥的活宝妹弱弱程序员。。。
                // Session userGateSession = NetInnerComponentSystem.Get(Root.Instance.Scene.GetComponent<NetInnerComponent>(), userGateIPEndPoint); 
                Session userGateSession = NetInnerComponent.Instance.Get(gateAppId); // 这里，我把 gateAppId 当作了 StartProcessConfig 里的 SceneId 来用的，标记进程号
                await userGateSession.Call(new R2G_PlayerKickOut_Req() { UserID = userId });
                Log.Info($"玩家{userId}已被踢下线");
            }
        }
    }
}  // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】