using ET;
using System;
namespace ET.Server {
    
    [MessageHandler(SceneType.Gate)]
    public class R2G_GetLoginKey_ReqHandler : AMRpcHandler<R2G_GetLoginKey_Req, G2R_GetLoginKey_Ack> {

        protected override void Run(Session session, R2G_GetLoginKey_Req message, Action<G2R_GetLoginKey_Ack> reply) {
            G2R_GetLoginKey_Ack response = new G2R_GetLoginKey_Ack();
            try {
                long key = RandomGenerator.RandInt64();
                // 这些【拖拉机游戏组件】，我需要找一个合适的地方，来添加这些组件。原是在 AppType.AllServer 下添加的, 可以加在【网关服】下
                Root.Instance.Scene.GetComponent<LandlordsGateSessionKeyComponent>().Add(key, message.UserID);
                response.Key = key;
                reply(response);
            }
            catch (Exception e) {
                ReplyError(response, e, reply);
            }
        }
    }
}
