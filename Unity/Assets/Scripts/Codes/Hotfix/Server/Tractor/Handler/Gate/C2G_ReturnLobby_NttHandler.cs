﻿using ET;
using System.Net;
namespace ET.Server {

    // 这个类是今天下午改，随便抓的一个类。过程是要：先把这个网关服的逻辑看懂，然后现重构后的框架里去找如何拿到相关服的地址，重构适配一下
    [MessageHandler(SceneType.Gate)] // 玩家申请什么进房间出厅全是与【地图服】交互
    public class C2G_ReturnLobby_NttHandler : AMHandler<C2G_ReturnLobby_Ntt> {
        protected override async ETTask Run(Session session, C2G_ReturnLobby_Ntt message) {
            // 验证Session
            if (!GateHelper.SignSession(session)) 
                return;
            User user = session.GetComponent<SessionUserComponent>().User; // User 类：带【是否正在匹配玩家过程中】住处，带ActorID 可发消息信息
            Scene scene = session.DomainScene();
            StartSceneConfig config = RealmGateAddressHelper.GetMatch(session.DomainZone()); // 又随便分配了一个【匹配服】：所以这里的逻辑是需要优化一下的【活宝妹就是一定要嫁给亲爱的表哥！！爱表哥，爱生活！！！】
            //StartConfigComponent config = Root.Instance.Scene.GetComponent<StartConfigComponent>();
            // ActorMessageSenderComponent actorProxyComponent = Root.Instance.Scene.GetComponent<ActorMessageSenderComponent>();
            // 正在匹配中发送玩家退出匹配请求
            if (user.IsMatching) { // 如果正在匹配房间，就去拿匹配服的地址
                // IPEndPoint matchIPEndPoint = config.MatchConfig.GetComponent<InnerConfig>().IPEndPoint; // 去找拿的方式
                // Session matchSession = Root.Instance.Scene.GetComponent<NetInnerComponent>().Get(matchIPEndPoint);
                // 【不知道】：这里是因为命名空间不对，还是怎样，这个组件 NetInnerComponent 挂在根控件 Root 场景下，却拿不到，调用不了。。找不到一个可以参考调用的例子
                // 把下面【会话框】的这一步也跳过
                // Session matchSession = NetInnerComponentSystem.Get(Root.Instance.Scene.GetComponent<NetInnerComponent>(), matchIPEndPoint); // 这里方法没有桥接起来，再查一下
                // Session matchSession = Root.Instance.Scene.GetComponent<NetInnerComponent>().Get(matchIPEndPoint);
                await scene.GetComponent<NetInnerComponent>().Get(config.InstanceId).Call(new G2M_PlayerExitMatch_Req() { UserID = user.UserID });
                // await matchSession.Call(new G2M_PlayerExitMatch_Req() { UserID = user.UserID });
                user.IsMatching = false; 
            }
            // 正在游戏中发送玩家退出房间请求
            if (user.ActorID != 0) {
                // ActorMessageSender actorProxy = actorProxyComponent.Get(user.ActorID);
                // await actorProxy.Call(new Actor_PlayerExitRoom_Req() { UserID = user.UserID });
                // 重构后，普通消息的发送，怎么发送的？
                await ActorMessageSenderComponent.Instance.Call(user.ActorID, new Actor_PlayerExitRoom_Req() { UserID = user.UserID });
                user.ActorID = 0;
            }
        }
    }
}