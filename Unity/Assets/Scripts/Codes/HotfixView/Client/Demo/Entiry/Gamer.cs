namespace ET.Client {
    // 玩家对象: 这个类，放的地方大概不对，要删除或是转个地方
    public sealed class Gamer : Entity {
        // 玩家唯一ID
        public long UserID;
        // { get; set; }
        // 是否准备
        public bool IsReady;
        // { get; set; }
    }
}
