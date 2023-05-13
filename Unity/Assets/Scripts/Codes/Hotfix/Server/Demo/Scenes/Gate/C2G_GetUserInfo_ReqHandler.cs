using System;
using ET;
using ETModel;
using ET.Server;
namespace ETHotfix {
    // 【网关服】：客户端请求玩家数据. 如果现在没有这些小服务器器了，我就需要再接着更改消息处理器。去看现框架里消息是如何分发处理的
    [MessageHandler(AppType.Gate)]
    public class C2G_GetUserInfo_ReqHandler : AMRpcHandler<C2G_GetUserInfo_Req, G2C_GetUserInfo_Ack> {

        protected override async void Run(Session session, C2G_GetUserInfo_Req message, Action<G2C_GetUserInfo_Ack> reply) {
            G2C_GetUserInfo_Ack response = new G2C_GetUserInfo_Ack();
            try {
                // 验证Session
                if (!GateHelper.SignSession(session)) {
                    response.Error = ErrorCode.ERR_SignError;
                    reply(response);
                    return;
                }
                // 查询用户信息
                DBProxyComponent dbProxyComponent = Game.Scene.GetComponent<DBProxyComponent>();
                UserInfo userInfo = await dbProxyComponent.Query<UserInfo>(message.UserID, false);
                response.NickName = userInfo.NickName;
                response.Wins = userInfo.Wins;
                response.Loses = userInfo.Loses;
                response.Money = userInfo.Money;
                reply(response);
            }
            catch (Exception e) {
                ReplyError(response, e, reply);
            }
        }
	}
}
