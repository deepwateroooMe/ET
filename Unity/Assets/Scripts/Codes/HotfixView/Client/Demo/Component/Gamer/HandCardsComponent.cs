using System.Collections.Generic;
using ETModel;
using UnityEngine;
using UnityEngine.UI;
namespace ETHotfix {
    [ObjectSystem]
    public class HandCardsComponentAwakeSystem : AwakeSystem<HandCardsComponent, GameObject> {
        public override void Awake(HandCardsComponent self, GameObject panel) {
            self.Awake(panel);
        }
    }
    // 某个玩家：手上的手牌，随游戏的进展会变少，每次出牌的时候，会看看牌型什么的
    public class HandCardsComponent : Component {
        public const string HANDCARD_NAME = "HandCard";
        public const string PLAYCARD_NAME = "PlayCard";

        private readonly Dictionary<string, GameObject> cardsSprite = new Dictionary<string, GameObject>();
        private readonly List<Card> handCards = new List<Card>();
        private readonly List<Card> playCards = new List<Card>(); // 这一轮：轮着这个玩家时，它想要出的牌
        private GameObject _poker;
        private GameObject _handCards;
        private Text _pokerNum;
        public GameObject Panel { get; private set; }
        public Identity AccessIdentity { get; set; }

        public void Awake(GameObject panel) {
            this.Panel = panel;
            _poker = this.Panel.Get<GameObject>("Poker");
            _handCards = this.Panel.Get<GameObject>("HandCards");
            _pokerNum = _poker?.GetComponentInChildren<Text>();
        }
        public override void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            base.Dispose();
            Reset();
        }
        // 重置
        public void Reset() {
            ClearHandCards();
            ClearPlayCards();
        }
        // 显示玩家游戏UI
        public void Appear() {
            _poker?.SetActive(true);
            _handCards?.SetActive(true);
        }
        // 隐藏玩家游戏UI: 什么情况下调用这个，玩家掉线下线了的时候？等待掉线的玩家上线的时候？
        public void Hide() {
            _poker?.SetActive(false);
            _handCards?.SetActive(false);
        }
        // 获取卡牌精灵：这是视图层的控件对应与操控，如何让这些排序的图片正确地反应游戏数据逻辑？
        public GameObject GetSprite(Card card) {
            GameObject cardSprite;
            cardsSprite.TryGetValue(card.GetName(), out cardSprite);
            return cardSprite;
        }
        // 设置手牌数量
        public void SetHandCardsNum(int num) {
            _pokerNum.text = num.ToString();
        }
        // 添加多张牌
        public void AddCards(IEnumerable<Card> cards) {
            foreach (Card card in cards) {
                AddCard(card);
            }
            CardsSpriteUpdate(handCards, 50.0f);
        }
        // 出多张牌
        public void PopCards(IList<Card> cards) {
            ClearPlayCards();
            foreach (Card card in cards) {
                PopCard(card);
            }
            CardsSpriteUpdate(playCards, 25.0f);
            CardsSpriteUpdate(handCards, 50.0f);
            // 同步剩余牌数
            GameObject poker = this.Panel.Get<GameObject>("Poker");
            if (poker != null) {
                Text pokerNum = poker.GetComponentInChildren<Text>();
                pokerNum.text = (int.Parse(pokerNum.text) - cards.Count).ToString();
            }
        }
        // 清空手牌
        public void ClearHandCards() {
            ClearCards(handCards);
        }
        // 清空出牌
        public void ClearPlayCards() {
            ClearCards(playCards);
        }
        // 卡牌精灵更新
        public void CardsSpriteUpdate(List<Card> cards, float interval) {
            if (cards.Count == 0) {
                return;
            }
            Sort(cards);
            float width = GetSprite(cards[0]).GetComponent<RectTransform>().sizeDelta.x;
            float startX = -((cards.Count - 1) * interval) / 2;
            for (int i = 0; i < cards.Count; i++) {
                RectTransform rect = GetSprite(cards[i]).GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(startX + (i * interval), rect.anchoredPosition.y);
            }
        }
        // 清空卡牌
        private void ClearCards(List<Card> cards) {
            for (int i = cards.Count - 1; i >= 0; i--) {
                Card card = cards[i];
                GameObject cardSprite = cardsSprite[card.GetName()];
                cardsSprite.Remove(card.GetName());
                cards.Remove(card);
                UnityEngine.Object.Destroy(cardSprite); // 这里是直接销毁，应该也是个浪费性能的地方。只要玩家不只玩一次，就会浪费性能？还是说游戏逻辑就是设定为只玩一次的，不考虑不支持连续玩？
            }
        }
        // 卡牌排序
        private void Sort(List<Card> cards) {
            CardHelper.Sort(cards);
            // 卡牌精灵层级排序
            for (int i = 0; i < cards.Count; i++) {
                GetSprite(cards[i]).transform.SetSiblingIndex(i);
            }
        }
        // 添加卡牌
        private void AddCard(Card card) {
            GameObject handCardSprite = CreateCardSprite(HANDCARD_NAME, card.GetName(), this.Panel.Get<GameObject>("HandCards").transform);
            handCardSprite.GetComponent<HandCardSprite>().Poker = card;
            cardsSprite.Add(card.GetName(), handCardSprite);
            handCards.Add(card);
        }
        // 出牌
        private void PopCard(Card card) {
            // 移除手牌
            if (handCards.Contains(card)) {
                GameObject handCardSprite = GetSprite(card);
                cardsSprite.Remove(card.GetName());
                handCards.Remove(card);
                UnityEngine.Object.Destroy(handCardSprite);
            }
            GameObject playCardSprite = CreateCardSprite(PLAYCARD_NAME, card.GetName(), this.Panel.Get<GameObject>("PlayCards").transform);
            cardsSprite.Add(card.GetName(), playCardSprite);
            playCards.Add(card);
        }
        // 创建卡牌精灵
        private GameObject CreateCardSprite(string prefabName, string cardName, Transform parent) {
            GameObject cardSpritePrefab = (GameObject)ETModel.Game.Scene.GetComponent<ResourcesComponent>().GetAsset($"{prefabName}.unity3d", prefabName);
            GameObject cardSprite = UnityEngine.Object.Instantiate(cardSpritePrefab);
            cardSprite.name = cardName;
            cardSprite.layer = LayerMask.NameToLayer("UI");
            cardSprite.transform.SetParent(parent.transform, false);
            Sprite sprite = CardHelper.GetCardSprite(cardName); // 这里是一副牌52 张的图片打在一个图集里，加载这个图集资源包，并获取相应牌的图片 Sprite
            cardSprite.GetComponent<Image>().sprite = sprite;
            return cardSprite;
        }
    }
}