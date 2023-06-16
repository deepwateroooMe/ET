using ET;
using System.Net;
namespace ET.Server {
    [MessageHandler(SceneType.Gate)] // 玩家申请什么进房间出厅全是与【地图服】交互
    public class C2G_ReturnLobby_NttHandler : AMHandler<C2G_ReturnLobby_Ntt> {
        protected override async ETTask Run(Session session, C2G_ReturnLobby_Ntt message) {
            // 验证Session
            if (!GateHelper.SignSession(session)) 
                return;
            User user = session.GetComponent<SessionUserComponent>().User;
            StartConfigComponent config = Root.Instance.Scene.GetComponent<StartConfigComponent>();
            ActorMessageSenderComponent actorProxyComponent = Root.Instance.Scene.GetComponent<ActorMessageSenderComponent>();
            // 正在匹配中发送玩家退出匹配请求
            if (user.IsMatching) { // 如果正在匹配房间，就去拿匹配服的地址
                IPEndPoint matchIPEndPoint = config.MatchConfig.GetComponent<InnerConfig>().IPEndPoint; // 去找拿的方式
                // Session matchSession = Root.Instance.Scene.GetComponent<NetInnerComponent>().Get(matchIPEndPoint);
                // 【不知道】：这里是因为命名空间不对，还是怎样，这个组件 NetInnerComponent 挂在根控件 Root 场景下，却拿不到，调用不了。。找不到一个可以参考调用的例子
                Session matchSession = NetInnerComponentSystem.Get(Root.Instance.Scene.GetComponent<NetInnerComponent>(), matchIPEndPoint); // 这里方法没有桥接起来，再查一下
                // Session matchSession = Root.Instance.Scene.GetComponent<NetInnerComponent>().Get(matchIPEndPoint);
                await matchSession.Call(new G2M_PlayerExitMatch_Req() { UserID = user.UserID });
                user.IsMatching = false;
            }
            // 正在游戏中发送玩家退出房间请求
            if (user.ActorID != 0) {
                ActorMessageSender actorProxy = actorProxyComponent.Get(user.ActorID);
                // ActorMessageSender actorProxy = ActorMessageSenderComponentSystem.Get(actorProxyComponent, user.ActorID); // 今天上午读的这个类，因为重构了，确实不是在说同一个方法。。
                await actorProxy.Call(new Actor_PlayerExitRoom_Req() { UserID = user.UserID });
                user.ActorID = 0;
            }
        }
    }
}