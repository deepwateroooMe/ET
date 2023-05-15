using System.Collections.Generic;
using System.Linq;
namespace ET.Server {

    // 房间状态
    public enum RoomState : byte {
        Idle,       
        Ready,      
        Game        
    }
    // 房间对象
    public sealed class Room : Entity {

        public readonly Dictionary<long, int> seats = new Dictionary<long, int>();
        public readonly Gamer[] gamers = new Gamer[3];
        // 房间状态
        public RoomState State { get; set; } = RoomState.Idle;
        // 房间玩家数量
        public int Count { get { return seats.Values.Count; } }

        public override void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            base.Dispose();
            seats.Clear();
            for (int i = 0; i < gamers.Length; i++) {
                if (gamers[i] != null) {
                    gamers[i].Dispose();
                    gamers[i] = null;
                }
            }
            State = RoomState.Idle;
        }
    }
}
