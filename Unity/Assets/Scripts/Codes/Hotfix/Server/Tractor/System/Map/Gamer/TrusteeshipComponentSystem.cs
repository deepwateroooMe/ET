using ET;
namespace ET.Server {
    [ObjectSystem]
    public class TrusteeshipComponentStartSystem : StartSystem<TrusteeshipComponent> {
        protected override void Start(TrusteeshipComponent self) {
            self.Start().Coroutine();
        }
    }
    public static class TrusteeshipComponentSystem {
        public static async ETTask Start(this TrusteeshipComponent self) {
            // 玩家所在房间
            Room room = RoomComponentSystem.Get(Root.Instance.Scene.GetComponent<RoomComponent>(), self.GetParent<Gamer>().RoomID);
            OrderControllerComponent orderController = room.GetComponent<OrderControllerComponent>();
            Gamer gamer = self.GetParent<Gamer>();
            bool isStartPlayCard = false;
            while (true) {
                await TimerComponent.Instance.WaitAsync(1000); // ET7 框架里重构后。单例TimerComponent.Instance 的写法
                if (self.IsDisposed) return;
                if (gamer.UserID != orderController?.CurrentAuthority) continue;
                // 自动出牌开关,用于托管延迟出牌
                isStartPlayCard = !isStartPlayCard;
                if (isStartPlayCard) continue;
                // 当还没抢地主时随机抢地主
                if (gamer.GetComponent<HandCardsComponent>().AccessIdentity == Identity.None) {
                    int randomSelect = RandomGenerator.RandomNumber(0, 2);
                    ActorMessageSenderComponent.Instance.Send(gamer.InstanceId, new Actor_GamerGrabLandlordSelect_Ntt() { IsGrab = randomSelect == 0 });
                    self.Playing = false;
                    continue;
                }
                // 自动提示出牌
                Actor_GamerPrompt_Ack response = await ActorMessageSenderComponent.Instance.Call(gamer.InstanceId, new Actor_GamerPrompt_Req()) as Actor_GamerPrompt_Ack;
                if (response.Error > 0 || response.Cards.Count == 0) 
                    ActorMessageSenderComponent.Instance.Send(gamer.InstanceId, new Actor_GamerDontPlay_Ntt());
                else  // 【下面的错误】：可能我的消息还是哪里弄错了，因为下面的没有要出的牌这个参数
                    ActorMessageSenderComponent.Instance.Send(gamer.InstanceId, new Actor_GamerPlayCard_Req() { Cards = response.Cards });
            }
        }
    }
}