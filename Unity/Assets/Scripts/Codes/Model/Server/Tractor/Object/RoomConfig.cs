namespace ET.Server {
    // 这里就默认有的房间认知：不同房间，有的玩大有的玩小；有的各种顠带子儿，有的可以什么也不带，所以需要一个基线来筛选各自房间标准的玩家
    // 房间配置
    public struct RoomConfig {
        // 房间初始倍率
        public int Multiples { get; set; }
        // 房间底分
        public long BasePointPerMatch { get; set; }
        // 房间最低门槛
        public long MinThreshold { get; set; }
    }
}
