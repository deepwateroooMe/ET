using System;
using ET;
namespace ET.Server {
    [MessageHandler(SceneType.Gate)]
    public class C2G_LoginGateHandler : AMRpcHandler<C2G_LoginGate, G2C_LoginGate> {
        protected override void Run(Session session, C2G_LoginGate message, Action<G2C_LoginGate> reply) {
            G2C_LoginGate response = new G2C_LoginGate();
            try {
                string account = Root.Instance.Scene.GetComponent<GateSessionKeyComponent>().Get(message.Key);
                if (account == null) {
                    response.Error = ErrorCore.ERR_ConnectGateKeyError;
                    response.Message = "Gate key验证失败!";
                    return;
                } 
                session.RemoveComponent<SessionAcceptTimeoutComponent>();
                PlayerComponent playerComponent = Root.Instance.Scene.GetComponent<PlayerComponent>();
Player player = playerComponent.AddChild<Player, string>(account);
                playerComponent.Add(player);
                session.AddComponent<SessionPlayerComponent>().PlayerId = player.Id;
                session.AddComponent<MailBoxComponent, MailboxType>(MailboxType.GateSession);
                response.PlayerId = player.Id;
                // await ETTask.CompletedTask;
                reply(response);
                session.Send(new G2C_TestHotfixMessage() { Info = "recv hotfix message success" });
            }
            catch (Exception e) {
                ReplyError(response, e, reply);
            }
        }
    }
}