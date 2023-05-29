using System.Collections.Generic;
namespace ET.Server {

    // 这些都算是：游戏逻辑控制的组件化拆分。以前自己的游戏可能是一个巨大无比的控制器文件，这里折分成了狠多个小组件控制
    [ComponentOf(typeof(Room))]
    public class OrderControllerComponent : Entity, IAwake {
        // 先手玩家
        public KeyValuePair<long, bool> FirstAuthority { get; set; }
        // 玩家抢地主状态
        public Dictionary<long, bool> GamerLandlordState = new Dictionary<long, bool>();
        // 本轮最大牌型玩家
        public long Biggest { get; set; }
        // 当前出牌玩家
        public long CurrentAuthority { get; set; }
        // 当前抢地主玩家
        public int SelectLordIndex { get; set; }
        // public override void Dispose() {
        //     if(this.IsDisposed) {
        //         return;
        //     }
        //     base.Dispose();
        //     this.GamerLandlordState.Clear();
        //     this.Biggest = 0;
        //     this.CurrentAuthority = 0;
        //     this.SelectLordIndex = 0;
        // }
    }
}
