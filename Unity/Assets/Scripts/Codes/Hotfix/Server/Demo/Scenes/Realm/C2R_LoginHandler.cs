using System;
using System.Net;

namespace ET.Server {

    [MessageHandler(SceneType.Realm)]
    public class C2R_LoginHandler : AMRpcHandler<C2R_Login, R2C_Login> {

// 这时百Realm网关层的登录过程: 它随机分配了一个实例网关GateId给你用,把这个网关GateId返回给客户端,以便接下来客户端可以拿这个网关ID 去建立映射
        protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response) {
            // 随机分配一个Gate:  就是一个区下有狠多个，随便抓一个分配给你
            StartSceneConfig config = RealmGateAddressHelper.GetGate(session.DomainZone()); // 就是今天早上看的那个图呀
            Log.Debug($"gate address: {MongoHelper.ToJson(config)}");
            // 一般一个区服下有一个Realm; 一个区服下一般有多个Gate，Realm通过与账户Id取模的方式固定分配给此账户一个Gate，向此Gate请求获取GateKey
            
            // 向gate请求一个key,客户端可以拿着这个key连接gate
            G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await ActorMessageSenderComponent.Instance.Call(
                config.InstanceId, new R2G_GetLoginKey() {Account = request.Account});
            response.Address = config.InnerIPOutPort.ToString();
            response.Key = g2RGetLoginKey.Key;
            response.GateId = g2RGetLoginKey.GateId;
        }
    }
}
