using System.Collections.Generic;
namespace ET.Server {

    // 房间管理组件
    public class RoomComponent : Entity, IAwake {

        private readonly Dictionary<long, Room> rooms = new Dictionary<long, Room>();
        // // 添加房间
        // public void Add(Room room) {
        //     this.rooms.Add(room.InstanceId, room);
        // }
        // // 获取房间
        // public Room Get(long id) {
        //     Room room;
        //     this.rooms.TryGetValue(id, out room);
        //     return room;
        // }
        // // 移除房间并返回
        // public Room Remove(long id) {
        //     Room room = Get(id);
        //     this.rooms.Remove(id);
        //     return room;
        // }
        // public override void Dispose() {
        //     if (this.IsDisposed) {
        //         return;
        //     }
        //     base.Dispose();
        //     foreach (var room in this.rooms.Values) {
        //         room.Dispose();
        //     }
        // }
    }
}
