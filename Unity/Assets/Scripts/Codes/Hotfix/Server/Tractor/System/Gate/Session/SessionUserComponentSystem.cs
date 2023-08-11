using ET;
using System.Net;
namespace ET.Server {
    // 【任何时候，活宝妹就是一定要嫁给亲爱的表哥！！！爱表哥，爱生活！！！】

    [ObjectSystem] // 这是个什么情况下，游戏中玩家可是是在发奈或是无奈，是要被游戏逻辑踢出去的状态？。。。
    public class SessionUserComponentDestroySystem : DestroySystem<SessionUserComponent> {
        protected override void Destroy(SessionUserComponent self) { 
            try {
                // 释放User对象时将User对象从管理组件中移除
                UserComponent userComponent = Root.Instance.Scene.GetComponent<UserComponent>();
                if (userComponent != null)
                    UserComponentSystem.Remove(userComponent, self.User.UserID); // 大概要跟客户端什么乱七八糟的Player 之类的一起弄。。
                // 这里，是要去拿【匹配服】，把【如果是，正在匹配中。。的玩家，剔除掉】，所以先去拿，匹配服的地址
                StartSceneConfig config = StartSceneConfigCategory.Instance.Match;
                //StartConfigComponent config = Root.Instance.Scene.GetComponent<StartConfigComponent>(); // 组件重构没了。。
                ActorMessageSenderComponent actorProxyComponent = Root.Instance.Scene.GetComponent<ActorMessageSenderComponent>();
                // 正在匹配中发送玩家退出匹配请求
                if (self.User.IsMatching) {
                    IPEndPoint matchIPEndPoint = config.InnerIPOutPort; // 它这里说，【匹配服】是唯一确定的，但是我弄出的是一条链表。。
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
                IPEndPoint realmIPEndPoint = StartSceneConfigCategory.Instance.Realm.InnerIPOutPort;
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