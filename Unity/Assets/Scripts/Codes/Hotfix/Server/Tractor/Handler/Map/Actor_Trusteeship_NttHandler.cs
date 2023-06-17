using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ET;
namespace ET.Server {
    [ActorMessageHandler(SceneType.Map)]
    public class Actor_Trusteeship_NttHandler : AMActorHandler<Gamer, Actor_Trusteeship_Ntt> {
        
        protected override void Run(Gamer gamer, Actor_Trusteeship_Ntt message) {
            // Room room = Root.Instance.Scene.GetComponent<RoomComponent>().Get(gamer.RoomID);
            Room room = RoomComponentSystem.Get(Root.Instance.Scene.GetComponent<RoomComponent>(), gamer.RoomID);
            // 是否已经托管
            bool isTrusteeship = gamer.GetComponent<TrusteeshipComponent>() != null;
            if (message.IsTrusteeship && !isTrusteeship) {
                gamer.AddComponent<TrusteeshipComponent>();
                Log.Info($"玩家{gamer.UserID}切换为自动模式");
            } else if (isTrusteeship) {
                gamer.RemoveComponent<TrusteeshipComponent>();
                Log.Info($"玩家{gamer.UserID}切换为手动模式");
            }
            // 这里由服务端设置消息UserID用于转发
            Actor_Trusteeship_Ntt transpond = new Actor_Trusteeship_Ntt();
            transpond.IsTrusteeship = message.IsTrusteeship;
            transpond.UserID = gamer.UserID;
            // 转发消息
            room.Broadcast(transpond);
            if (isTrusteeship) {
                OrderControllerComponent orderController = room.GetComponent<OrderControllerComponent>();
                if (gamer.UserID == orderController.CurrentAuthority) {
                    bool isFirst = gamer.UserID == orderController.Biggest;
                    // ActorMessageSender actorProxy = gamer.GetComponent<UnitGateComponent>().GetActorMessageSender();
                    // actorProxy.Send(new Actor_AuthorityPlayCard_Ntt() { UserID = orderController.CurrentAuthority, IsFirst = isFirst });
                    // 感觉下面改的，可能正常是繁杂，总感觉是一个框架，应该有更为系统化的方法来调用，或只是亲爱的表哥的活宝妹现在的脑袋对这块儿还有点儿糊糊。。。
                    ActorMessageSenderComponent.Instance.Send(gamer.GetComponent<UnitGateComponent>().GateSessionActorId,
                                                                    new Actor_AuthorityPlayCard_Ntt() { UserID = orderController.CurrentAuthority, IsFirst = isFirst });
                }
            }
        }
    }
}