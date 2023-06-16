using ET;
using System;
namespace ET.Server {
    [MessageHandler(SceneType.Gate)]
    public class R2G_GetLoginKey_ReqHandler : AMRpcHandler<R2G_GetLoginKey_Req, G2R_GetLoginKey_Ack> {

        // 自己瞎改的，必机时再查一下源代码 
        protected override async ETTask Run(Session session, R2G_GetLoginKey_Req message, G2R_GetLoginKey_Ack response) {
            long key = RandomGenerator.RandInt64();
            // 这些【拖拉机游戏组件】，我需要找一个合适的地方，来添加这些组件。原是在 AppType.AllServer 下添加的, 可以加在【网关服】下
            LandlordsGateSessionKeyComponentSystem.Add(Root.Instance.Scene.GetComponent<LandlordsGateSessionKeyComponent>(), key, message.UserID);
            response.Key = key;
            await ETTask.CompletedTask;
        }
    }
}