using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
namespace ET.Client {

    // 牌类
    public partial class Card : IEquatable<Card> {

        public bool Equals(Card other) { // 数字与花型 
            return this.CardWeight == other.CardWeight && this.CardSuits == other.CardSuits;
        }
        // 获取卡牌名
        public string GetName() {
            return this.CardSuits == Suits.None ? this.CardWeight.ToString() : $"{this.CardSuits.ToString()}{this.CardWeight.ToString()}";
        }
    }
}
