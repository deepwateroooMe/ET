// using ET.Server;
using System.Collections.Generic;
namespace ET {
    // 为什么需要这个组件，没想明白
    [ComponentOf(typeof(Room))]
    public class DeskCardsCacheComponent : Entity, IAwake {
        // 牌桌上的牌
        public readonly List<Card> library = new List<Card>();
        // 地主牌
        public readonly List<Card> LordCards = new List<Card>();
        // 牌桌上的总牌数
        public int CardsCount { get { return this.library.Count; } }
        // 当前最大牌型: 这里为什么要纪录当前最大牌型？哪家的？
        public CardsType Rule { get; set; }
        // 牌桌上最小的牌
        public int MinWeight { get { return (int)this.library[0].CardWeight; } }
    }
}