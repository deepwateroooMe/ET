using ET;
using System.Net;
namespace ET.Server {
    [ObjectSystem]
    public class SessionUserComponentDestroySystem : DestroySystem<SessionUserComponent> {
        // 【任何时候，活宝妹就是一定要嫁给亲爱的表哥！！！爱表哥，爱生活！！！】
// 这种，现在狠好改，可以把它们全部消灭干净。。但是不知道改对了没有，只有运行一次之后，才能确定？
        protected override void Destroy(SessionUserComponent self) { 
            try {
                // 释放User对象时将User对象从管理组件中移除
                UserComponent userComponent = Root.Instance.Scene.GetComponent<UserComponent>();
                if (userComponent != null)// 感觉这个写法好土，先让它这样。。
                    UserComponentSystem.Remove(userComponent, self.User.UserID); // 大概要跟客户端什么乱七八糟的Player 之类的一起弄。。
                StartConfigComponent config = Root.Instance.Scene.GetComponent<StartConfigComponent>(); // 组件重构没了。。
                ActorMessageSenderComponent actorProxyComponent = Root.Instance.Scene.GetComponent<ActorMessageSenderComponent>();
                // 正在匹配中发送玩家退出匹配请求
                if (self.User.IsMatching) {
                    IPEndPoint matchIPEndPoint = config.MatchConfig.GetComponent<InnerConfig>().IPEndPoint;
                    Session matchSession = NetInnerComponentSystem.Get(Root.Instance.Scene.GetComponent<NetInnerComponent>(), matchIPEndPoint);
                    // await matchSession.Call(new G2M_PlayerExitMatch_Req() { UserID = self.User.UserID });
                    matchSession.Call(new G2M_PlayerExitMatch_Req() { UserID = self.User.UserID }).Coroutine();
                }
                // 正在游戏中发送玩家退出房间请求
                if (self.User.ActorID != 0) {
                    // ActorMessageSender actorProxy = actorProxyComponent.Get(self.User.ActorID);
                    // // await actorProxy.Call(new Actor_PlayerExitRoom_Req() { UserID = self.User.UserID });
                    // actorProxy.Call(new Actor_PlayerExitRoom_Req() { UserID = self.User.UserID }).Coroutine();
                    ActorMessageSenderComponent.Instance.Call(self.User.ActorID, new Actor_PlayerExitRoom_Req() { UserID = self.User.UserID }).Coroutine();
                }
                // 向登录服务器发送玩家下线消息
                IPEndPoint realmIPEndPoint = config.RealmConfig.GetComponent<InnerConfig>().IPEndPoint;
                Session realmSession = NetInnerComponentSystem.Get(Root.Instance.Scene.GetComponent<NetInnerComponent>(), realmIPEndPoint);
                // await realmSession.Call(new G2R_PlayerOffline_Req() { UserID = self.User.UserID });
                realmSession.Call(new G2R_PlayerOffline_Req() { UserID = self.User.UserID }).Coroutine();
                self.User.Dispose();
                self.User = null;
            }
            catch (System.Exception e) {
                Log.Trace(e.ToString());
            }
        }
    }
}