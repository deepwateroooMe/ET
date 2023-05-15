using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
namespace ET.Client {

    [FriendOf(typeof(UILobbyComponent))]
    public static class UILobbyComponentSystem {

        [ObjectSystem]
        public class UILobbyComponentAwakeSystem: AwakeSystem<UILobbyComponent> {
            protected override void Awake(UILobbyComponent self) { // 这个不是异步程序，所以放这里面对报错
                ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

                // 【三个按钮】：选择房间模式，匹配服为玩家匹配，玩家自已创建一个特性拖拉机，或是加入他知道的亲朋好友拖拉机房
                self.matchRoom = rc.Get<GameObject>("MatchRoom"); 
                self.matchRoom.GetComponent<Button>().onClick.AddListener(() => { self.matchRoom().Coroutine(); });
                self.enterRoom = rc.Get<GameObject>("EnterRoom"); 
                self.enterRoom.GetComponent<Button>().onClick.AddListener(() => { self.enterRoom().Coroutine(); });
                self.createRoom = rc.Get<GameObject>("CreateRoom"); 
                self.createRoom.GetComponent<Button>().onClick.AddListener(() => { self.createRoom().Coroutine(); });
                InitLobbyComponent(self); // 补充：获取玩家数据【因为登录成功，才进到这里厅里的】
            }
        }
        private static async void InitLobbyComponent(this UILobbyComponent self) {
        // 获取玩家数据: 按说应该是注册登录服的逻辑，或者是数据库服存放着用户信息，都是通过Gate中转
// 去理：客户端相关的这些东西，组件，与客户端场景
            long userId = self.ClientScene().GetComponent<PlayerComponent>().MyId;
            // long userId = ClientComponent.Instance.LocalPlayer.UserID; // 当地玩家：是前一步，客户端登录成功的时候设置的
            C2G_GetUserInfo_Req c2G_GetUserInfo_Req = new C2G_GetUserInfo_Req() { UserID = userId }; // 去从网关服拿玩家信息
// 下面的：SessionComponent.Instance.Session.Call 重构了。所以不再是以前的单例，而有生成系，重新去找，客户端不同场景SceneType间发消息是怎么发的？
            G2C_GetUserInfo_Ack g2C_GetUserInfo_Ack = await self.ClientScene().GetComponent<SessionComponent>().Session.Call(c2G_GetUserInfo_Req) as G2C_GetUserInfo_Ack;
            // 显示用户信息
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            rc.Get<GameObject>("NickName").GetComponent<Text>().text = g2C_GetUserInfo_Ack.NickName;
            rc.Get<GameObject>("Money").GetComponent<Text>().text = g2C_GetUserInfo_Ack.Money.ToString();                
        }
        // 【回调：】自定义三个按钮的回调。这些个过程流程，就主要参考，同框架的斗地主游戏
        public static async ETTask matchRoom(this UILobbyComponent self) { // 通过网关服中转，请求匹配服为给匹配一个房间四人桌
            try {
                // 发送开始匹配消息【待处理】：匹配服的逻辑处理没有检查，可能会不通
                C2G_StartMatch_Req c2G_StartMatch_Req = new C2G_StartMatch_Req();
                G2C_StartMatch_Ack g2C_StartMatch_Ack = await self.ClientScene().GetComponent<SessionComponent>().Session.Call(c2G_StartMatch_Req) as G2C_StartMatch_Ack; 
                if (g2C_StartMatch_Ack.Error == ErrorCode.ERR_UserMoneyLessError) {
                    Log.Error("余额不足"); // 玩家余额不足
                    return;
                }
                // 匹配成功了：UI 界面切换，切换到房间界面【UI 事件系统】：改成发布事件。因为这个事件，应该会被三个按钮都会触发
                EventSystem.Instance.Publish(self.ClientScene(), new EventType.ModeSelected()); // 这个，再去找下，谁在订阅这个事件，如何带动游戏开启的状态？

                // 将房间设为匹配状态【这里的逻辑，再去检查】创建房间的时候，添加的这个组件，先去完成创建房间，需要专用工厂来生产
                // room.GetComponent<TractorRoomComponent>().Matching = true;// LandlordsRoomComponent 【绝对不是在这里设置的！！】
            }
            catch (Exception e) {
                Log.Error(e.ToString());
            }
        }
        // 接下来，这两个选项，暂时不处理
        public static async ETTask enterRoom(this UILobbyComponent self) { // 不知道，这个，与 EnterMap 有没有本质的区别，要【检查一下】
            await EnterRoomHelper.EnterRoomAsync(self.ClientScene());
            await UIHelper.Remove(self.ClientScene(), UIType.UILobby); // 移除大厅控件 
        }
        public static async ETTask createRoom(this UILobbyComponent self) {

        }
    }
}