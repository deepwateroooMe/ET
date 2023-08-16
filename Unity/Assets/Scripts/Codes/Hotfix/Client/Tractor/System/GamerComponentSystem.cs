using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ET;
namespace ET { 
// 【爱表哥，爱生活！！任何时候，活宝妹就是一定要嫁给亲爱的表哥！！爱表哥，爱生活！！！】
    [ObjectSystem] // 只有这样，生成系，System 才能与固定层桥接起来？爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
    public class GamerComponentAwakeSystem : AwakeSystem<GamerComponent> {
        protected override void Awake(GamerComponent self) {
            self.seats = new Dictionary<long, int>();
            self.gamers = new Gamer[4];
        }
    }
// 玩家组件管理生成系：
    [FriendOf(typeof(GamerComponent))]
    public static class GamerComponentSystem {
        // public static LocalGamer() { get; set; } 
// 提供给房间组件用的：就是当前玩家。。。这里狠奇怪：当有一个当前玩家，就是说，它应该是背在某个玩家身上，是客户端组件？！！

        // 添加玩家
        public static void Add(GamerComponent self, Gamer gamer, int seatIndex) {
           self.gamers[seatIndex] = gamer;
           self.seats[gamer.UserID] = seatIndex;
        }
        // 获取玩家
        public static Gamer Get(GamerComponent self, long id) {
            int seatIndex = GetGamerSeat(self, id);
            if (seatIndex >= 0) {
                return self.gamers[seatIndex];
            }
            return null;
        }
        // 获取所有玩家
        public static Gamer[] GetAll(GamerComponent self) {
            return self.gamers;
        }
        // 获取玩家座位索引
        public static int GetGamerSeat(GamerComponent self, long id) {
            int seatIndex;
            if (self.seats.TryGetValue(id, out seatIndex)) {
                return seatIndex;
            }
            return -1;
        }
        // 移除玩家并返回
        public static Gamer Remove(GamerComponent self, long id) {
            int seatIndex = GetGamerSeat(self, id);
            if (seatIndex >= 0) {
                Gamer gamer = self.gamers[seatIndex];
                self.gamers[seatIndex] = null;
                self.seats.Remove(id);
                return gamer;
            }
            return null;
        }
    }
}
