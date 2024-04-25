using System;
namespace ET.Server {
    [MessageHandler(SceneType.Gate)]
    public class C2G_LoginGateHandler : AMRpcHandler<C2G_LoginGate, G2C_LoginGate> {
        protected override async ETTask Run(Session session, C2G_LoginGate request, G2C_LoginGate response) {
            Scene scene = session.DomainScene();
            string account = scene.GetComponent<GateSessionKeyComponent>().Get(request.Key);
            if (account == null) {
                response.Error = ErrorCore.ERR_ConnectGateKeyError;
                response.Message = "Gate key验证失败!";
                return;
            }
            session.RemoveComponent<SessionAcceptTimeoutComponent>();
            PlayerComponent playerComponent = scene.GetComponent<PlayerComponent>();
            Player player = playerComponent.AddChild<Player, string>(account);
            player.AddComponent<PlayerSessionComponent>().Session = session;
			// 每个游戏玩家，身背邮箱，可以收发消息 
            player.AddComponent<MailBoxComponent, MailboxType>(MailboxType.GateSession);
			// 每个【进程上的、位置服场景】：负责管理，本进程上，所有场景下的，有必要、需要、想要，向位置服，注册上报位置信息的实例
            await player.AddLocation(LocationType.Player); // 向【进程上的、位置服】注册位置信息。全框架，只有这一个使用用例 AddLocation()
            session.AddComponent<SessionPlayerComponent>().Player = player;
            response.PlayerId = player.Id;
            await ETTask.CompletedTask;
        }
    }
}