using ET;
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
                // Session matchSession = Game.Scene.GetComponent<NetInnerComponent>().Get(matchIPEndPoint);
                // 【不知道】：这里是因为命名空间不对，还是怎样，这个组件 NetInnerComponent 挂在根控件 Root 场景下，却拿不到，调用不了。。找不到一个可以参考调用的例子
                Session matchSession = Root.Instance.Scene.GetComponent<NetInnerComponent>().Get(matchIPEndPoint);
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
