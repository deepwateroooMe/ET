using System;
using System.Net;
namespace ET.Server {
	// 处理特定消息类型的、场景类型；
	// 同一场景，可以有多个、各司其职、处理各种不同类型消息的、消息处理器；
	// 同一消息类型，应该只有特定场景类型来专职处理
	[MessageHandler(SceneType.Realm)]
    public class C2R_LoginHandler : AMRpcHandler<C2R_Login, R2C_Login> {
        protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response) {
            // 随机分配一个Gate
            StartSceneConfig config = RealmGateAddressHelper.GetGate(session.DomainZone());
            Log.Debug($"gate address: {MongoHelper.ToJson(config)}");
            // 向gate请求一个key,客户端可以拿着这个key连接gate
            G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await ActorMessageSenderComponent.Instance.Call(
                config.InstanceId, new R2G_GetLoginKey() {Account = request.Account});
            response.Address = config.InnerIPOutPort.ToString();
            response.Key = g2RGetLoginKey.Key;
            response.GateId = g2RGetLoginKey.GateId;
        }
    }
}
