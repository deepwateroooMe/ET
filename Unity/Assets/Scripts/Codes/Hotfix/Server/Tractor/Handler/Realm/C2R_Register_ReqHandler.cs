using System;
using ET;
using System.Collections.Generic;
namespace ET.Server {
 // 游戏服务器： Realm 注册登录服，把这个注册，自动登录，存数据库等的整个过程弄清楚，服务器端连接链路
    [MessageHandler(SceneType.Realm)]
    public class C2R_Register_ReqHandler : AMRpcHandler<C2R_Register_Req, R2C_Register_Ack> {
        protected override async ETTask Run(Session session, C2R_Register_Req message, R2C_Register_Ack response) {
            // 数据库操作对象: 代理索引
            DBProxyComponent dbProxy = Root.Instance.Scene.GetComponent<DBProxyComponent>();
            // 查询账号是否存在
            List<ComponentWithId> result = await dbProxy.Query<AccountInfo>(_account => _account.Account == message.Account);
            if (result.Count > 0) { // 出错：该帐户已注册
                response.Error = ErrorCode.ERR_AccountAlreadyRegister;
                // reply(response); // <<<<<<<<<<<<<<<<<<<< 重构后，不需要手动发返回消息. 异常是ETTash 里的封装会给抛出
                return;
            }
            // 新建账号
            AccountInfo newAccount = ComponentFactory.CreateWithId<AccountInfo>(IdGenerater.GenerateId());
            newAccount.Account = message.Account;
            newAccount.Password = message.Password;
            Log.Info($"注册新账号：{MongoHelper.ToJson(newAccount)}");
            // 新建用户信息
            UserInfo newUser = ComponentFactory.CreateWithId<UserInfo>(newAccount.Id);
            newUser.NickName = $"用户{message.Account}";
            newUser.Money = 10000;
            // 保存到数据库: 内网不同服务器之间的交互
            await dbProxy.Save(newAccount);  // 异步保存到数据库服务器
            await dbProxy.Save(newUser, false);
        }
    }
}