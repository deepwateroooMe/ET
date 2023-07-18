using System.Collections.Generic;
using System.Linq;
using ET.Server;
namespace ET {
    // 房间状态
    public enum RoomState : byte {
        Idle,       
        Ready,      
        Game        
    }
    // 房间对象
    [ChildOf(typeof(RoomComponent))]
    public sealed class Room : Entity, IAwake<long> {
        public long id; // 自己给它加的：房间门牌号，身份证号

        public readonly Dictionary<long, int> seats = new Dictionary<long, int>();
        public readonly Gamer[] gamers = new Gamer[3];
        // 房间状态
        public RoomState State { get; set; } = RoomState.Idle;
        // 房间玩家数量
        public int Count { get { return seats.Values.Count; } }
    }
}
