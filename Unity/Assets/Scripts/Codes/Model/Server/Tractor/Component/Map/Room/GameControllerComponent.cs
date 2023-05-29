namespace ET.Server {
    // 感觉个类，更多的是【一座桥】：把游戏的这个单位级件，全连接起来
    [ComponentOf(typeof(Room))]
    public class GameControllerComponent : Entity, IAwake<RoomConfig> {

        // 房间配置
        public RoomConfig Config { get; set; }
        // 底分: 这里呈现出与房间的这些设置不一致的状态。是说，三个玩家，可以在既定房间的基础上提升玩乐标准？
        public long BasePointPerMatch { get; set; }
        // 全场倍率
        public int Multiples { get; set; }
        // 最低入场门槛
        public long MinThreshold { get; set; }

        // public override void Dispose() {
        //     if(this.IsDisposed) {
        //         return;
        //     }
        //     base.Dispose();
        //     this.BasePointPerMatch = 0;
        //     this.Multiples = 0;
        //     this.MinThreshold = 0;
        // }
    }
}
