using System;
using Google.Protobuf;
namespace ET {
    public partial class Card : IEquatable<Card> {    // 牌类
        public bool Equals(Card other) { // 数字与花型 
            return this.CardWeight == other.CardWeight && this.CardSuits == other.CardSuits;
        }
        public string GetName() { // 获取卡牌名
            return this.CardSuits == Suits.None ? this.CardWeight.ToString() : $"{this.CardSuits.ToString()}{this.CardWeight.ToString()}";
        }
    }
}
