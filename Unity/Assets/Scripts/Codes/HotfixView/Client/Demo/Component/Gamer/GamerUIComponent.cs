using System;
using UnityEngine;
using UnityEngine.UI;
using ET;
namespace ET.Client {
    [ObjectSystem]
    public class GamerUIComponentStartSystem : StartSystem<GamerUIComponent> {
        protected override void Start(GamerUIComponent self) {
            self.Start();
        }
    }
    // 玩家UI组件
    [ComponentOf(typeof(Gamer))]
    public class GamerUIComponent : Entity, IStart { 
        // UI面板
        public GameObject Panel { get; private set; }
        // 玩家昵称
        public string NickName { get { return name.text; } }
        private Image headPhoto;
        private Text prompt;
        private Text name;
        private Text money;
        public void Start() {
            if (this.GetParent<Gamer>().IsReady) 
                SetReady();
        }
        // 重置面板
        public void ResetPanel() {
            ResetPrompt();
            this.headPhoto.gameObject.SetActive(false);
            this.name.text = "空位";
            this.money.text = "";
            this.Panel = null;
            this.prompt = null;
            this.name = null;
            this.money = null;
            this.headPhoto = null;
        }
        // 设置面板
        public void SetPanel(GameObject panel) {
            this.Panel = panel;
            // 绑定关联
            this.prompt = this.Panel.Get<GameObject>("Prompt").GetComponent<Text>();
            this.name = this.Panel.Get<GameObject>("Name").GetComponent<Text>();
            this.money = this.Panel.Get<GameObject>("Money").GetComponent<Text>();
            this.headPhoto = this.Panel.Get<GameObject>("HeadPhoto").GetComponent<Image>();
            UpdatePanel();
        }
        // 更新面板
        public void UpdatePanel() {
            if (this.Panel != null) {
                SetUserInfo();
                headPhoto.gameObject.SetActive(false);
            }
        }
        // 设置玩家身份
        public void SetIdentity(Identity identity) {
            if (identity == Identity.None) return;
            string spriteName = $"Identity_{Enum.GetName(typeof(Identity), identity)}";
            Sprite headSprite = CardHelper.GetCardSprite(spriteName);
            headPhoto.sprite = headSprite;
            headPhoto.gameObject.SetActive(true);
        }
        // 玩家准备
        public void SetReady() {
            prompt.text = "准备！";
        }
        // 出牌错误
        public void SetPlayCardsError() {
            prompt.text = "您出的牌不符合规则！";
        }
        // 玩家不出
        public void SetDiscard() {
            prompt.text = "不出";
        }
        // 打2 时，玩家抢不抢庄：或者去想，玩家要不要反主牌花色
        public void SetGrab(GrabLandlordState state) {
            switch (state) {
            case GrabLandlordState.Not:
                break;
            case GrabLandlordState.Grab:
                prompt.text = "抢地主";
                break;
            case GrabLandlordState.UnGrab:
                prompt.text = "不抢";
                break;
            }
        }
        public void ResetPrompt() { // 重置提示
            prompt.text = "";
        }
        public void GameStart() { // 游戏开始
            ResetPrompt();
        }
        private async void SetUserInfo() { // 设置用户信息
            G2C_GetUserInfo_Ack g2C_GetUserInfo_Ack = await SessionComponent.Instance.Session.Call(new C2G_GetUserInfo_Req() { UserID = this.GetParent<Gamer>().UserID }) as G2C_GetUserInfo_Ack;
            if (this.Panel != null) {
                name.text = g2C_GetUserInfo_Ack.NickName;
                money.text = g2C_GetUserInfo_Ack.Money.ToString();
            }
        }
        public override void Dispose() {
            if (this.IsDisposed) return;
            base.Dispose();
            ResetPanel(); // 重置玩家UI
        }
    }
}