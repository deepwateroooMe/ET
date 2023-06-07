using System.Collections.Generic;
namespace ET.Server {

    // 房间管理组件
    [ComponentOf(typeof(Scene))] // 
    public static class RoomComponentSystem {
        // private readonly Dictionary<long, Room> rooms = new Dictionary<long, Room>();
        // 添加房间
        public static void Add(RoomComponent self, Room room) {
            self.rooms.Add(room.InstanceId, room);
        }
        // 获取房间
        public static Room Get(RoomComponent self, long id) {
            Room room;
            self.rooms.TryGetValue(id, out room);
            return room;
        }
        // 移除房间并返回
        public static Room Remove(RoomComponent self, long id) {
            Room room = Get(self, id);
            self.rooms.Remove(id);
            return room;
        }
        // public static override void Dispose() {
        //     if (self.IsDisposed) {
        //         return;
        //     }
        //     base.Dispose();
        //     foreach (var room in self.rooms.Values) {
        //         room.Dispose();
        //     }
        // }
    }
}
