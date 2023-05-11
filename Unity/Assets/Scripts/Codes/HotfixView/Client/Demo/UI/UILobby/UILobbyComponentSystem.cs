using UnityEngine;
using UnityEngine.UI;
namespace ET.Client {

    [FriendOf(typeof(UILobbyComponent))]
    public static class UILobbyComponentSystem {

        [ObjectSystem]
        public class UILobbyComponentAwakeSystem: AwakeSystem<UILobbyComponent> {
            protected override void Awake(UILobbyComponent self) {
                ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

                // 【三个按钮】：选择房间模式，匹配服为玩家匹配，玩家自已创建一个特性拖拉机，或是加入他知道的亲朋好友拖拉机房
                self.matchRoom = rc.Get<GameObject>("MatchRoom"); 
                self.matchRoom.GetComponent<Button>().onClick.AddListener(() => { self.MatchRoom().Coroutine(); });
                self.enterRoom = rc.Get<GameObject>("EnterRoom"); 
                self.enterRoom.GetComponent<Button>().onClick.AddListener(() => { self.EnterRoom().Coroutine(); });
                self.createRoom = rc.Get<GameObject>("CreateRoom"); 
                self.createRoom.GetComponent<Button>().onClick.AddListener(() => { self.CreateRoom().Coroutine(); });
            }
        }
        // 【回调：】自定义三个按钮的回调
        public static async ETTask matchMatch(this UILobbyComponent self) { // 通过网关服中转，请求匹配服为给匹配一个房间四人桌

        }
        public static async ETTask enterRoom(this UILobbyComponent self) {
            await EnterMapHelper.EnterRoomAsync(self.ClientScene());
            await UIHelper.Remove(self.ClientScene(), UIType.UILobby);
        }
        public static async ETTask createRoom(this UILobbyComponent self) {

        }

        // Prev: 用作参考
        public static async ETTask EnterMap(this UILobbyComponent self) {
            await EnterMapHelper.EnterMapAsync(self.ClientScene());
            await UIHelper.Remove(self.ClientScene(), UIType.UILobby);
        }
    }
}