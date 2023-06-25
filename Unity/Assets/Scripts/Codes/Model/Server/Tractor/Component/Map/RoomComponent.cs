using System.Collections.Generic;
namespace ET.Server {
    // 房间管理组件
    [ComponentOf(typeof(Scene))]
    public class RoomComponent : Entity, IAwake {
        // 感觉上面的系统没有添加完整，要怎么才能够创建一个新房间呢？
        public Dictionary<long, Room> rooms = new Dictionary<long, Room>();
    }
}
