// 不知道这里，为什么它说，找不到 Gamer 类，先改成是ET 命名空间。原本写的是服务端
// namespace ET.Server {
namespace ET {
    // [ObjectSystem]
    // public class GamerAwakeSystem : AwakeSystem<Gamer,long> {
    //     protected override void Awake(Gamer self, long id) {
    //         self.Awake(id);
    //         // self.UserID = id;
    //     }
    // }
    // 房间玩家对象
    [ChildOf(typeof(GamerComponent))] // 先把这个去掉：这就是刚才的那个报错了
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