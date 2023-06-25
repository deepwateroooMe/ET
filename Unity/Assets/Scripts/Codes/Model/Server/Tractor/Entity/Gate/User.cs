namespace ET.Server {
    // 玩家对象
    
    [ChildOf(typeof(UserComponent))]
    public sealed class User : Entity, IAwake<long> {
        // 用户ID（唯一）
        public long UserID { get; set; }
        // 是否正在匹配中
        public bool IsMatching { get; set; }
        // Gate转发ActorID
        public long ActorID { get; set; }
    }
}