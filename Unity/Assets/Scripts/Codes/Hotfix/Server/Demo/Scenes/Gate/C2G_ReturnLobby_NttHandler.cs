using ET;
using ET.Server;
using System.Net;
namespace ET.Server {

    [MessageHandler(SceneType.Gate)]
    public class C2G_ReturnLobby_NttHandler : AMHandler<C2G_ReturnLobby_Ntt> { // 这是晚一点儿，玩家从拖拉机房出来的服务器热更新处理

        protected override async void Run(Session session, C2G_ReturnLobby_Ntt message) {
            // 验证Session
            if (!GateHelper.SignSession(session)) {
                return;
            }
            User user = session.GetComponent<SessionUserComponent>().User;
            StartConfigComponent config = Game.Scene.GetComponent<StartConfigComponent>();
            ActorMessageSenderComponent actorProxyComponent = Game.Scene.GetComponent<ActorMessageSenderComponent>();
            // 正在匹配中发送玩家退出匹配请求
            if (user.IsMatching) {
                IPEndPoint matchIPEndPoint = config.MatchConfig.GetComponent<InnerConfig>().IPEndPoint;
                Session matchSession = Game.Scene.GetComponent<NetInnerComponent>().Get(matchIPEndPoint);
                await matchSession.Call(new G2M_PlayerExitMatch_Req() { UserID = user.UserID });
                user.IsMatching = false;
            }
            // 正在游戏中发送玩家退出房间请求
            if (user.ActorID != 0) {
                ActorMessageSender actorProxy = actorProxyComponent.Get(user.ActorID);
                await actorProxy.Call(new Actor_PlayerExitRoom_Req() { UserID = user.UserID });
                user.ActorID = 0;
            }
        }
    }
}