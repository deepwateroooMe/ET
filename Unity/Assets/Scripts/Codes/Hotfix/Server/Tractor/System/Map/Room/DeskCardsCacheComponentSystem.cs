﻿// using ET;
using System.Collections.Generic;
namespace ET.Server {

    [FriendOfAttribute(typeof(ET.DeskCardsCacheComponent))]
    public static class DeskCardsCacheComponentSystem {
        // 获取总权值
        public static int GetTotalWeight(this DeskCardsCacheComponent self) {
            return GetWeight(self.library.ToArray(), self.Rule);
        }
        // 获取牌桌所有牌
        public static Card[] GetAll(this DeskCardsCacheComponent self) {
            return self.library.ToArray();
        }
        // 发牌
        public static Card Deal(this DeskCardsCacheComponent self) {
            Card card = self.library[self.CardsCount - 1];
            self.library.Remove(card);
            return card;
        }
        // 向牌库中添加牌
        public static void AddCard(this DeskCardsCacheComponent self, Card card) {
            self.library.Add(card);
        }
        // 清空牌桌
        public static void Clear(this DeskCardsCacheComponent self) {
            DeckComponent deck = self.GetParent<Room>().GetComponent<DeckComponent>();
            while (self.CardsCount > 0) {
                Card card = self.library[self.CardsCount - 1];
                self.library.Remove(card);
                deck.AddCard(card);
            }
            self.Rule = CardsType.None;
        }
        // 手牌排序
        public static void Sort(this DeskCardsCacheComponent self) {
            SortCards(self.library); // 这个静态方法也得搬过来
        }

        // 【CardsHelper】里的静态方法：两个静态方法, 搬过来，免得它报环形依赖的错！！【任何时候，活宝妹就是一定要嫁给亲爱的表哥！！爱表哥，爱生活！！！】
        // 卡组排序
        public static void SortCards(List<Card> cards) {
            cards.Sort(
                (Card a, Card b) => {
// 先按照权重降序，再按花色升序
                    return -a.CardWeight.CompareTo(b.CardWeight) * 2 +
                        a.CardWeight.CompareTo(b.CardWeight);
                }
                );
        }
        public static int GetWeight(IList<Card> cards, CardsType rule) {
            int totalWeight = 0;
            if (rule == CardsType.JokerBoom) {
                totalWeight = int.MaxValue;
            } else if (rule == CardsType.Boom) {
                totalWeight = (int)cards[0].CardWeight * (int)cards[1].CardWeight * (int)cards[2].CardWeight * (int)cards[3].CardWeight + (int.MaxValue / 2);
            } else if (rule == CardsType.ThreeAndOne || rule == CardsType.ThreeAndTwo) {
                for (int i = 0; i < cards.Count; i++) {
                    if (i < cards.Count - 2) {
                        if (cards[i].CardWeight == cards[i + 1].CardWeight &&
                            cards[i].CardSuits == cards[i + 2].CardSuits) {
                            totalWeight += (int)cards[i].CardWeight;
                            totalWeight *= 3;
                            break;
                        }
                    }
                }
            } else {
                for (int i = 0; i < cards.Count; i++) 
                    totalWeight += (int)cards[i].CardWeight;
            }
            return totalWeight;
        }
    }
}
