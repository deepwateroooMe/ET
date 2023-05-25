using System.Collections.Generic;

namespace ET {

    // 牌库组件: 这里还有个配置忘记了：黑认是两副牌的拖拉机，可是好像还有更多副牌的，要考虑一下吗？
    public class DeckComponent : Entity, IAwake {
        // 牌库中的牌
        public readonly List<Card> library = new List<Card>();
        // 牌库中的总牌数
        public int CardsCount { get { return this.library.Count; } }

        // public override void Dispose() {
        //     if(this.IsDisposed) {
        //         return;
        //     }
        //     base.Dispose();
        //     library.Clear();
        // }
    }
}