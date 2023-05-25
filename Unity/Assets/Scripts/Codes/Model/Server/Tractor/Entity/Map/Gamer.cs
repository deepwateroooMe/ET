namespace ET.Server {
    // [ObjectSystem]
    // public class GamerAwakeSystem : AwakeSystem<Gamer,long> {
    //     protected override void Awake(Gamer self, long id) {
    //         self.Awake(id);
    //         // self.UserID = id;
    //     }
    // }
    // 房间玩家对象
    public sealed class Gamer : Entity, IAwake<long> {
    // public sealed class Gamer : Entity {
        // 用户ID（唯一）
        public long UserID { get; set; }
        // 玩家GateActorID
        public long PlayerID { get; set; }
        // 玩家所在房间ID
        public long RoomID { get; set; }
        // 是否准备
        public bool IsReady { get; set; }
        // 是否离线
        public bool isOffline { get; set; }
    }
}