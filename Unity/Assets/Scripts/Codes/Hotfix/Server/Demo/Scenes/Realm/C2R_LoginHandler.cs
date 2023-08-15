using System;
using System.Net;
namespace ET.Server {
    [MessageHandler(SceneType.Realm)]
    public class C2R_LoginHandler : AMRpcHandler<C2R_Login, R2C_Login> {
        // 【ET7 的进一步精简封装】：封装在AMRpcHandler 抽象实现类里
        protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response) {
			// 随机分配一个Gate? 这里不再随机了吧，分的是，来自于【客户端会话框】所在区的对应区的【网关服】。就是分小区管理. 小区里N 个中随机分配一个
			StartSceneConfig config = RealmGateAddressHelper.GetGate(session.DomainZone());
			Log.Debug($"gate address: {MongoHelper.ToJson(config)}");
			
			// 向gate请求一个key,客户端可以拿着这个key连接gate.
            // 这里，使用【消息发送器单例类】为桥梁再封装调用。【消息发送器单例类】会帮助框架，自动封装返回消息
			G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await ActorMessageSenderComponent.Instance.Call(
				config.InstanceId, new R2G_GetLoginKey() {Account = request.Account});

			response.Address = config.InnerIPOutPort.ToString(); // 现在的问题就成为是：什么地方、什么组件可以记这些信息？【客户端】拿到这些信息的，所有客户端要做的就是，与【网关服】建立一个通话【会话框】呀，就不用记什么了
			response.Key = g2RGetLoginKey.Key;
			response.GateId = g2RGetLoginKey.GateId;
		}
    }
}