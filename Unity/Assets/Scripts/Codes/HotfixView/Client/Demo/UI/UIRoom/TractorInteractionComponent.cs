using UnityEngine;
using UnityEngine.UI;
using ETModel;
using System.Collections.Generic;

namespace ET.Client {

    [ObjectSystem]
    public class TractorInteractionComponentAwakeSystem : AwakeSystem<TractorInteractionComponent> {
        protected override void Awake(TractorInteractionComponent self) {
            self.Awake();
        }
    }

    public class TractorInteractionComponent : Entity, IAwake {

        private Button playButton;
        private Button promptButton;
        private Button discardButton;
        private Button grabButton;
        private Button disgrabButton;
        private Button changeGameModeButton;
        private List<Card> currentSelectCards = new List<Card>();

        public bool isTrusteeship { get; set; }
        public bool IsFirst { get; set; }

        public void Awake() {
            ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            playButton = rc.Get<GameObject>("PlayButton").GetComponent<Button>();
            promptButton = rc.Get<GameObject>("PromptButton").GetComponent<Button>();
            discardButton = rc.Get<GameObject>("DiscardButton").GetComponent<Button>();
            grabButton = rc.Get<GameObject>("GrabButton").GetComponent<Button>();
            disgrabButton = rc.Get<GameObject>("DisgrabButton").GetComponent<Button>();
            changeGameModeButton = rc.Get<GameObject>("ChangeGameModeButton").GetComponent<Button>();
            // 绑定事件
            playButton.onClick.Add(OnPlay);
            promptButton.onClick.Add(OnPrompt);
            discardButton.onClick.Add(OnDiscard);
            grabButton.onClick.Add(OnGrab);
            disgrabButton.onClick.Add(OnDisgrab);
            changeGameModeButton.onClick.Add(OnChangeGameMode);
            // 默认隐藏UI
            playButton.gameObject.SetActive(false);
            promptButton.gameObject.SetActive(false);
            discardButton.gameObject.SetActive(false);
            grabButton.gameObject.SetActive(false);
            disgrabButton.gameObject.SetActive(false);
            changeGameModeButton.gameObject.SetActive(false);
        }
        public override void Dispose() {
            if(this.IsDisposed) {
                return;
            }
            base.Dispose();
            ETModel.Game.Scene.GetComponent<ResourcesComponent>()?.UnloadBundle($"{UIType.TractorInteraction}.unity3d");
        }
        // 游戏结束
        public void Gameover() {
            changeGameModeButton.gameObject.SetActive(false);
        }
        // 开始游戏
        public void GameStart() {
            isTrusteeship = false;
            changeGameModeButton.GetComponentInChildren<Text>().text = "自动";
            changeGameModeButton.gameObject.SetActive(true);
        }
        // 选中卡牌
        public void SelectCard(Card card) {
            currentSelectCards.Add(card);
        }
        // 取消选中卡牌
        public void CancelCard(Card card) {
            currentSelectCards.Remove(card);
        }
        // 清空选中卡牌
        public void Clear() {
            currentSelectCards.Clear();
        }
        // 开始抢地主
        public void StartGrab() {
            grabButton.gameObject.SetActive(true);
            disgrabButton.gameObject.SetActive(true);
        }
        // 开始出牌
        public void StartPlay() {
            if (isTrusteeship) {
                playButton.gameObject.SetActive(false);
                promptButton.gameObject.SetActive(false);
                discardButton.gameObject.SetActive(false);
            } else {
                playButton.gameObject.SetActive(true);
                promptButton.gameObject.SetActive(!IsFirst);
                discardButton.gameObject.SetActive(!IsFirst);
            }
        }
        // 结束抢地主
        public void EndGrab() {
            grabButton.gameObject.SetActive(false);
            disgrabButton.gameObject.SetActive(false);
        }
        // 结束出牌
        public void EndPlay() {
            playButton.gameObject.SetActive(false);
            promptButton.gameObject.SetActive(false);
            discardButton.gameObject.SetActive(false);
        }
        // 切换游戏模式
        private void OnChangeGameMode() {
            if (isTrusteeship) {
                StartPlay();
                changeGameModeButton.GetComponentInChildren<Text>().text = "托管";
            } else {
                EndPlay();
                changeGameModeButton.GetComponentInChildren<Text>().text = "取消托管";
            }
            SessionComponent.Instance.Session.Send(new Actor_Trusteeship_Ntt() { IsTrusteeship = !this.isTrusteeship });
        }
        // 出牌
        private async void OnPlay() {
            CardHelper.Sort(currentSelectCards);
            Actor_GamerPlayCard_Req request = new Actor_GamerPlayCard_Req();
            request.Cards.AddRange(currentSelectCards);
            Actor_GamerPlayCard_Ack response = await SessionComponent.Instance.Session.Call(request) as Actor_GamerPlayCard_Ack;
            // 出牌错误提示
            GamerUIComponent gamerUI = Game.Scene.GetComponent<UIComponent>().Get(UIType.TractorRoom).GetComponent<GamerComponent>().LocalGamer.GetComponent<GamerUIComponent>();
            if (response.Error == ErrorCode.ERR_PlayCardError) {
                gamerUI.SetPlayCardsError();
            }
        }
        // 提示
        private async void OnPrompt() {
            Actor_GamerPrompt_Req request = new Actor_GamerPrompt_Req();
            Actor_GamerPrompt_Ack response = await SessionComponent.Instance.Session.Call(request) as Actor_GamerPrompt_Ack;
            GamerComponent gamerComponent = this.GetParent<UI>().GetParent<UI>().GetComponent<GamerComponent>();
            HandCardsComponent handCards = gamerComponent.LocalGamer.GetComponent<HandCardsComponent>();
            // 清空当前选中
            while (currentSelectCards.Count > 0) {
                Card selectCard = currentSelectCards[currentSelectCards.Count - 1];
                handCards.GetSprite(selectCard).GetComponent<HandCardSprite>().OnClick(null);
            }
            // 自动选中提示出牌
            if (response.Cards != null) {
                foreach (Card card in response.Cards) {
                    handCards.GetSprite(card).GetComponent<HandCardSprite>().OnClick(null);
                }
            }
        }
        // 不出
        private void OnDiscard() {
            SessionComponent.Instance.Session.Send(new Actor_GamerDontPlay_Ntt());
        }
        // 抢地主
        private void OnGrab() {
            SessionComponent.Instance.Session.Send(new Actor_GamerGrabLandlordSelect_Ntt() { IsGrab = true });
        }
        // 不抢
        private void OnDisgrab() {
            SessionComponent.Instance.Session.Send(new Actor_GamerGrabLandlordSelect_Ntt() { IsGrab = false });
        }
    }
}
