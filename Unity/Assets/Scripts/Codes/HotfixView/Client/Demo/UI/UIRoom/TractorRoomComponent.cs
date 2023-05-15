using System;
using ET;
using UnityEngine;
using UnityEngine.UI;
namespace ET.Client {
    [ObjectSystem]
    public class TractorRoomComponentAwakeSystem : AwakeSystem<TractorRoomComponent> {
        protected override void Awake(TractorRoomComponent self) {
            self.Awake(self);
        }
    }
    public class TractorRoomComponent : Entity, IAwake {
        private TractorInteractionComponent interaction;
        private Text multiples;
        public readonly GameObject[] GamersPanel = new GameObject[4];
        public bool Matching { get; set; }
        public TractorInteractionComponent Interaction {
            get {
                if (interaction == null) {
                    UI uiRoom = this.GetParent<UI>();
                    UI uiInteraction = TractorInteractionFactory.Create(UIType.TractorInteraction, uiRoom);
                    interaction = uiInteraction.GetComponent<TractorInteractionComponent>();
                }
                return interaction;
            }
        }
        public override void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            base.Dispose();
            this.Matching = false;
            this.interaction = null;
        }

        public void Awake(TractorRoomComponent self) { 
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            GameObject quitButton = rc.Get<GameObject>("QuitButton");   // 退出： 退出房间,不玩了
            GameObject readyButton = rc.Get<GameObject>("ReadyButton"); // 准备:  准备开始玩儿
            GameObject multiplesObj = rc.Get<GameObject>("Multiples");
            multiples = multiplesObj.GetComponent<Text>();
            // 绑定事件
            quitButton.GetComponent<Button>().onClick.AddListener(() => { OnQuit(self).Coroutine(); });
            // readyButton.GetComponent<Button>().onClick.Add(OnReady);
            readyButton.GetComponent<Button>().onClick.AddListener(() => { OnReady(self).Coroutine(); });
            
            // 默认隐藏UI: ，隐藏倍率/准备按钮/牌桌（地主3张牌）
            multiplesObj.SetActive(false);
            readyButton.SetActive(false);
            rc.Get<GameObject>("Desk").SetActive(false);
            // 添加玩家面板
            GameObject gamersPanel = rc.Get<GameObject>("Gamers");
            this.GamersPanel[0] = gamersPanel.Get<GameObject>("Left");
            this.GamersPanel[1] = gamersPanel.Get<GameObject>("Local");
            this.GamersPanel[2] = gamersPanel.Get<GameObject>("Right");
            // 添加本地玩家
            User localPlayer = ClientComponent.Instance.LocalPlayer;
            Gamer localGamer = GamerFactory.Create(localPlayer.UserID, false);
            AddGamer(localGamer, 1);
            this.GetParent<UI>().GetComponent<GamerComponent>().LocalGamer = localGamer;
        }
        // 添加玩家
        public void AddGamer(Gamer gamer, int index) {
            GetParent<UI>().GetComponent<GamerComponent>().Add(gamer, index);
            // 【游戏视图上】：每个玩家自己有个小画板，来显示每个玩家，比如自己出的牌，叫过反过的主，等，小UI 面板
            gamer.GetComponent<GamerUIComponent>().SetPanel(this.GamersPanel[index]);
        }
        // 移除玩家
        public void RemoveGamer(long id) {
            Gamer gamer = GetParent<UI>().GetComponent<GamerComponent>().Remove(id);
            gamer.Dispose();
        }
        // 设置倍率
        public void SetMultiples(int multiples) {
            this.multiples.gameObject.SetActive(true);
            this.multiples.text = multiples.ToString();
        }
        // 重置倍率
        public void ResetMultiples() {
            this.multiples.gameObject.SetActive(false);
            this.multiples.text = "1";
        }
        // 退出房间
        private static async ETTask OnQuit(TractorRoomComponent self) {
            // 发送退出房间消息: 要去大厅
            self.ClientScene().GetComponent<SessionComponent>().Session.Send(new C2G_ReturnLobby_Ntt());
            // 切换到大厅界面【不等结果吗？】也该是发布一个自定义的事件 TODO
            Game.Scene.GetComponent<UIComponent>().Create(UIType.UILobby);
            Game.Scene.GetComponent<UIComponent>().Remove(UIType.TractorRoom);
        }
        // 准备
        private static async ETTask OnReady(TractorRoomComponent self) {
            // 发送准备:  发送Actor_GamerReady_Ntt消息。 玩家加入匹配队列/退出匹配队列的逻辑均在服务端完成，客户端在不需要具体动作时都不会有变化。
            self.ClientScene().GetComponent<SessionComponent>().Session.Send(new Actor_GamerReady_Ntt());
        }
    }
}