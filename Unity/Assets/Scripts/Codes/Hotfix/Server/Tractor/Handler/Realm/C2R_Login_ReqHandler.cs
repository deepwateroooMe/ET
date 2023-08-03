using System;
using ET;
using System.Collections.Generic;
namespace ET.Server {
    // 这个文件，自己后来添加的，在热更域服务端游戏文件夹里，这个文件需要删除，逻辑不完整
    
    [MessageHandler(SceneType.Realm)] // 这个类是，后来自己又添加上去的，可能与原 ET7 框架的示例有重合，可以删去
    public class C2R_Login_ReqHandler : AMRpcHandler<C2R_Login_Req, R2C_Login_Ack> {
        protected override async ETTask Run(Session session, C2R_Login_Req message, R2C_Login_Ack response) {
            DBComponent dbComponent = DBManagerComponentSystem.GetZoneDB(Root.Instance.Scene.GetComponent<DBManagerComponent>(), session.DomainZone());
            Log.Info($"登录请求：{{Account:'{message.Account}',Password:'{message.Password}'}}");
            // 验证账号密码是否正确: 同样的用户名与密码，数据库可以拿到【同用户名、同密码】的所有用户有链表，不是感觉狠奇怪吗？同用户名密码，不该是只有一个吗？
            List<AccountInfo> result = await dbComponent.Query<AccountInfo>(_account => _account.Account == message.Account && _account.Password == message.Password);
            if (result.Count == 0) {
                response.Error = ErrorCode.ERR_LoginError;
                return;
            }
            AccountInfo account = result[0] as AccountInfo;
            Log.Info($"账号登录成功{MongoHelper.ToJson(account)}");
            // 将已在线玩家踢下线
            await RealmHelper.KickOutPlayer(account.Id);
// 这里，需要去检查一下：后面的逻辑，是否，被自己当初改编译错误的时候，去掉了：后面的逻辑是需要的，但是自己还没能改出来，就先看另一个文件 
            // // 随机分配网关服务器: ET7 下这个模块重构了，应该不再需要 RealmGateAddressComponent 这个组件了
            // StartConfig gateConfig = Root.Instance.Scene.GetComponent<RealmGateAddressComponent>().GetAddress();
            // Session gateSession = Root.Instance.Scene.GetComponent<NetInnerComponent>().Get(gateConfig.GetComponent<InnerConfig>().IPEndPoint);
            // // 请求登录Gate服务器密匙
            // G2R_GetLoginKey_Ack getLoginKey_Ack = await gateSession.Call(new R2G_GetLoginKey_Req() { UserID = account.Id }) as G2R_GetLoginKey_Ack;
            // response.Key = getLoginKey_Ack.Key;
            // response.Address = gateConfig.GetComponent<OuterConfig>().Address2;
        }
    }
}