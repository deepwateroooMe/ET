using System.Linq;
using System.Collections.Generic;
namespace ET.Client { // 现在应该去想：ET 命名空间对吗，组件管理类？它为什么是在HotfixView 里？
    // 组件：是提供给房间用，用来管理游戏中每个房间里的最多三个当前玩家
    public class GamerComponent : Entity, IAwake { // 它也有【生成系】：要把生成系类，补上 GamerComponentSystem.cs
        private readonly Dictionary<long, int> seats = new Dictionary<long, int>();
        private readonly Gamer[] gamers = new Gamer[4]; 
        public Gamer LocalGamer { get; set; } // 提供给房间组件用的：就是当前玩家。。。

        // // 添加玩家
        // public void Add(Gamer gamer, int seatIndex) {
        //     gamers[seatIndex] = gamer;
        //     seats[gamer.UserID] = seatIndex;
        // }
        // // 获取玩家
        // public Gamer Get(long id) {
        //     int seatIndex = GetGamerSeat(id);
        //     if (seatIndex >= 0) {
        //         return gamers[seatIndex];
        //     }
        //     return null;
        // }
        // // 获取所有玩家
        // public Gamer[] GetAll() {
        //     return gamers;
        // }
        // // 获取玩家座位索引
        // public int GetGamerSeat(long id) {
        //     int seatIndex;
        //     if (seats.TryGetValue(id, out seatIndex)) {
        //         return seatIndex;
        //     }
        //     return -1;
        // }
        // // 移除玩家并返回
        // public Gamer Remove(long id) {
        //     int seatIndex = GetGamerSeat(id);
        //     if (seatIndex >= 0) {
        //         Gamer gamer = gamers[seatIndex];
        //         gamers[seatIndex] = null;
        //         seats.Remove(id);
        //         return gamer;
        //     }
        //     return null;
        // }
    }
}
