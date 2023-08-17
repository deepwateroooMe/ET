using System.Collections.Generic;
using ET;
using UnityEngine;
namespace ET.Client {
    public static class CardHelper {
        // 卡牌图集预设名称
        public const string ATLAS_NAME = "Atlas";
        // 排序
        public static void Sort(List<Card> cards) {
            for (int i = cards.Count; i > 0; i--) {
                for (int j = 0; j < i - 1; j++) {
                    // 先按照权重降序，再按花色升序: 就是按照玩牌的大小顺序，再花色
                    if (-CompareTo((int)cards[j].CardWeight, (int)cards[j + 1].CardWeight) * 2 +
                        CompareTo((int)cards[j].CardSuits, (int)cards[j + 1].CardSuits) > 0) {
                        Card temp = cards[j];
                        cards[j] = cards[j + 1];   
                        cards[j + 1] = temp;
                    }
                }
            }
            // cards.Sort((a, b) => {
            //    // 先按照权重降序，再按花色升序
            //    return -a.CardWeight.CompareTo(b.CardWeight) * 2 +
            //        a.CardSuits.CompareTo(b.CardSuits);
            // });
        }
        // int比较
        public static int CompareTo(int a, int b) {
            int result;
            if (a > b) {
                result = 1;
            } else if (a < b) {
                result = -1;
            } else {
                result = 0;
            }
            return result;
        }
        // 获取卡牌精灵
        public static Sprite GetCardSprite(string cardName) {
            GameObject atlas = (GameObject)Root.Instance.Scene.GetComponent<ResourcesComponent>().GetAsset($"{ATLAS_NAME}.unity3d", ATLAS_NAME);
            return atlas.Get<Sprite>(cardName);
        }
    }
}