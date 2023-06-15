using System;
using ET;
namespace ET.Server {
    [MessageHandler(SceneType.Gate)]
    public class C2G_GetUserInfo_ReqHandler : AMRpcHandler<C2G_GetUserInfo_Req, G2C_GetUserInfo_Ack> {
		protected override async ETTask Run(Session session, C2G_GetUserInfo_Req request, G2C_GetUserInfo_Ack response) {
        // protected override async void Run(Session session, C2G_GetUserInfo_Req message, Action<G2C_GetUserInfo_Ack> reply) {
            // G2C_GetUserInfo_Ack response = new G2C_GetUserInfo_Ack();
            // try {
                // 验证Session
                if (!GateHelper.SignSession(session)) {
                    response.Error = ErrorCode.ERR_SignError;
                    // reply(response);
                    return;
                }
                // 查询用户信息: 【数据库相关的部分】：是自己还没能弄懂、没能整合进来的模块。查找一下原版本，是否有此类。我自己添加的游戏逻辑
                DBProxyComponent dbProxyComponent = Root.Instance.Scene.GetComponent<DBProxyComponent>(); // 组件的场景，可能没写对。。。
                UserInfo userInfo = await dbProxyComponent.Query<UserInfo>(message.UserID, false); // 重复文件？ UserInfo
                response.NickName = userInfo.NickName;
                response.Wins = userInfo.Wins;
                response.Loses = userInfo.Loses;
                response.Money = userInfo.Money;
            //     reply(response);
            // }
            // catch (Exception e) {
            //     ReplyError(response, e, reply);
            // }
        }
	}
}