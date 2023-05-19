using System;
using ET;
using System.Net;
namespace ET.Server {
    [MessageHandler(SceneType.Gate)]
    public class C2G_LoginGate_ReqHandler : AMRpcHandler<C2G_LoginGate_Req, G2C_LoginGate_Ack> {
        protected override async void Run(Session session, C2G_LoginGate_Req message, Action<G2C_LoginGate_Ack> reply) {
            G2C_LoginGate_Ack response = new G2C_LoginGate_Ack();
            try {
                LandlordsGateSessionKeyComponent landlordsGateSessionKeyComponent = session.DomainScene().GetComponent<LandlordsGateSessionKeyComponent>();
                long userId = landlordsGateSessionKeyComponent.Get(message.Key);
                // 验证登录Key是否正确
                if (userId == 0) {
                    response.Error = ErrorCode.ERR_ConnectGateKeyError;
                    reply(response);
                    return;
                }
                // Key过期
                landlordsGateSessionKeyComponent.Remove(message.Key);
                // 创建User对象
                User user = UserFactory.Create(userId, session.InstanceId);
                await user.AddComponent<MailBoxComponent>().AddLocation(); // 为【用户】添加了邮箱和位置信息。那么这个用户就可以收发消息了
                // 添加User对象关联到Session上
                session.AddComponent<SessionUserComponent>().User = user;
                await session.AddComponent<MailBoxComponent>().AddLocation(); // 添加消息转发组件
                StartConfigComponent config = Root.Instance.Scene.GetComponent<StartConfigComponent>();
                IPEndPoint realmIPEndPoint = config.RealmConfig.GetComponent<InnerConfig>().IPEndPoint;
                Session realmSession = session.DomainScene().GetComponent<NetInnerComponent>().Get(realmIPEndPoint);
                await realmSession.Call(new G2R_PlayerOnline_Req() { UserID = userId, GateAppID = config.StartConfig.AppId });
                response.PlayerID = user.InstanceId;
                response.UserID = user.UserID;
                reply(response);
            }
            catch (Exception e) {
                ReplyError(response, e, reply);
            }
        }
    }
}