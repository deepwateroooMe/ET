namespace ET.Client {
    // 玩家对象
    public sealed class User : Entity, IAwake<long> {
        // 用户ID（唯一）
        public long UserID { get; set; }
    }
}