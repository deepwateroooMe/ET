using System;
using ET;
namespace ET.Server {
    [MessageHandler(SceneType.Gate)]
    public class C2G_GetUserInfo_ReqHandler : AMRpcHandler<C2G_GetUserInfo_Req, G2C_GetUserInfo_Ack> {
		protected override async ETTask Run(Session session, C2G_GetUserInfo_Req request, G2C_GetUserInfo_Ack response) {
            // 验证Session: C2G_GetUserInfo_Req 包括 RpcID ＋ UserId
            if (!GateHelper.SignSession(session)) {
                response.Error = ErrorCode.ERR_SignError;
                return;
            }
            // 查询用户信息: 先要想办法（从会话框可以拿到吗），得到用户所在的小区编号，才能根据此小区号拿到该区服下的数据库组件DBComponent. 这样就算改好一个了呀。。。
            // DBProxyComponent dbProxyComponent = Root.Instance.Scene.GetComponent<DBProxyComponent>(); // 组件的场景，可能没写对。。。
            // UserInfo userInfo = await dbProxyComponent.Query<UserInfo>(request.UserID, false); // 重复文件？ UserInfo
            DBComponent dbComponent = DBManagerComponentSystem.GetZoneDB(Root.Instance.Scene.GetComponent<DBManagerComponent>(), session.DomainZone());
            UserInfo userInfo = await dbComponent.Query<UserInfo>(request.UserID);
            response.NickName = userInfo.NickName;
            response.Wins = userInfo.Wins;
            response.Loses = userInfo.Loses;
            response.Money = userInfo.Money;
        }
	}
}