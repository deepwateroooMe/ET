using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ET;
namespace ET.Server {
    [ActorMessageHandler(SceneType.Map)]
    public class Actor_GamerDontPlay_NttHandler : AMActorHandler<Gamer, Actor_GamerDontPlay_Ntt> {

        protected override void Run(Gamer gamer, Actor_GamerDontPlay_Ntt message) { 
            // 【去确认RoomComponent】到底是在哪里添加的，哪个场景下什么时候添加的？在Programs.cs 全服下添加的房间管理组件；当没有全服，要怎么办呢？是不是全服，自己加一个全服不就可以了吗？
            Room room = Root.Instance.Scene.GetComponent<RoomComponent>().Get(gamer.RoomID);
            OrderControllerComponent orderController = room.GetComponent<OrderControllerComponent>();
            if (orderController.CurrentAuthority == gamer.UserID) {
                // 转发玩家不出牌消息
                Actor_GamerDontPlay_Ntt transpond = new Actor_GamerDontPlay_Ntt();
                transpond.UserID = gamer.UserID;
                room.Broadcast(transpond);
                // 轮到下位玩家出牌
                orderController.Turn();
                // 判断是否先手
                bool isFirst = orderController.CurrentAuthority == orderController.Biggest;
                if (isFirst) {
                    room.GetComponent<DeskCardsCacheComponent>().Clear();
                }
                room.Broadcast(new Actor_AuthorityPlayCard_Ntt() { UserID = orderController.CurrentAuthority, IsFirst = isFirst });
            }
        }
    }
}
