using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ET;
namespace ET.Client { 
// 【爱表哥，爱生活！！任何时候，活宝妹就是一定要嫁给亲爱的表哥！！爱表哥，爱生活！！！】
// 玩家组件管理生成系：
    public static class GamerComponentSystem {
        [ObjectSystem]
        // 不明白：这里，为什么会找不到组件申明类，下午家里看一下.
        // 狠奇怪，难道这个系统还有同步不到位的问题？还是如昨天 .csproj 项目里引入文件的问题，下午要运行测试一下
        // 乌龟王八蛋：调了半天，等了那么久，它就自已以好了，一群死乌龟王八蛋贱畜牲，操纵帐户的？！！！
        // 乌龟王八蛋：它就自已以好了, 它自己又滚没了，一群死乌龟王八蛋贱畜牲，操纵帐户的？！！！都它妈的不得好死！！！
        public class GamerComponentAwakeSystem : AwakeSystem<GamerComponent> {
            protected override void Awake(GamerComponent self) {
                self.seats = new Dictionary<long, int>();
                self.gamers = new Gamer[4];
            }
        }
        // public static LocalGamer() { get; set; } 
// 提供给房间组件用的：就是当前玩家。。。这里狠奇怪：当有一个当前玩家，就是说，它应该是背在某个玩家身上，是客户端组件？！！

        // 添加玩家
        public static void Add(GamerComponent self, Gamer gamer, int seatIndex) {
           self.gamers[seatIndex] = gamer;
           self.seats[gamer.UserID] = seatIndex;
        }
        // 获取玩家
        public static Gamer Get(GamerComponent self, long id) {
            int seatIndex = GetGamerSeat(id);
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
            int seatIndex = GetGamerSeat(id);
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
