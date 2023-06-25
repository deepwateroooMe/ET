using System.Collections.Generic;
namespace ET.Server {

    // 房间管理组件
    [ComponentOf(typeof(Scene))] 
    [FriendOfAttribute(typeof(ET.Server.RoomComponent))]
    public static class RoomComponentSystem {
        // 用这个作自己的测试的例子，给它添加 Awake() 方法，来尝试唤醒它的热更新层的生成系统？找个例子来看。。。
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<RoomComponent> {
            protected override void Awake(RoomComponent self) {
                self.rooms = new Dictionary<long, Room>();
            }
        }
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
    }
}
