using System;
using ET;
namespace ET.Server {
    public static class OrderControllerComponentSystem {
        // 初始化
        public static void Init(this OrderControllerComponent self, long id) {
            self.FirstAuthority = new System.Collections.Generic.KeyValuePair<long, bool>(id, false);
            self.Biggest = 0;
            self.CurrentAuthority = id;
            self.SelectLordIndex = 1;
            self.GamerLandlordState.Clear();
        }
        // 开始
        public static void Start(this OrderControllerComponent self, long id) {
            self.Biggest = id;
            self.CurrentAuthority = id;
        }
        // 轮转: 就是把出牌权，移至按顺序的下一个玩家
        public static void Turn(this OrderControllerComponent self) {
            Room room = self.GetParent<Room>();
            Gamer[] gamers = room.GetAll();
            int index = Array.FindIndex(gamers, (gamer) => self.CurrentAuthority == gamer.UserID);
            index++;
            if (index == gamers.Length) {
                index = 0;
            }
            self.CurrentAuthority = gamers[index].UserID;
        }
    }
}
