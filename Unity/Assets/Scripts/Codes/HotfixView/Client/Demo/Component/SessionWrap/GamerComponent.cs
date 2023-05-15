using System.Linq;
using System.Collections.Generic;
namespace ET.Client {

    // 组件：是提供给房间用，用来管理游戏中每个房间里的最多三个当前玩家
    public class GamerComponent : Entity, IAwake {

        private readonly Dictionary<long, int> seats = new Dictionary<long, int>();
        private readonly Gamer[] gamers = new Gamer[3]; // 这里纪录：跟它一起玩的同一个房间里的三个小伙伴。。。

        public Gamer LocalGamer { get; set; } // 提供给房间组件用的：就是当前应该出牌的玩家。。。
        // 添加玩家
        public void Add(Gamer gamer, int seatIndex) {
            gamers[seatIndex] = gamer;
            seats[gamer.UserID] = seatIndex;
        }
        // 获取玩家
        public Gamer Get(long id) {
            int seatIndex = GetGamerSeat(id);
            if (seatIndex >= 0) {
                return gamers[seatIndex];
            }
            return null;
        }
        // 获取所有玩家
        public Gamer[] GetAll() {
            return gamers;
        }
        // 获取玩家座位索引
        public int GetGamerSeat(long id) {
            int seatIndex;
            if (seats.TryGetValue(id, out seatIndex)) {
                return seatIndex;
            }
            return -1;
        }
        // 移除玩家并返回
        public Gamer Remove(long id) {
            int seatIndex = GetGamerSeat(id);
            if (seatIndex >= 0) {
                Gamer gamer = gamers[seatIndex];
                gamers[seatIndex] = null;
                seats.Remove(id);
                return gamer;
            }
            return null;
        }
        public override void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            base.Dispose();
            this.LocalGamer = null;
            this.seats.Clear();
            for (int i = 0; i < this.gamers.Length; i++) {
                if (gamers[i] != null) {
                    gamers[i].Dispose();
                    gamers[i] = null;
                }
            }
        }
    }
}