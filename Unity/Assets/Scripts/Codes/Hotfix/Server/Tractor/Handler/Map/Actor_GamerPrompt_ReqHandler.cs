using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ET;
namespace ET.Server {
    [ActorMessageHandler(SceneType.Map)]
    public class Actor_GamerPrompt_ReqHandler : AMActorRpcHandler<Gamer, Actor_GamerPrompt_Req, Actor_GamerPrompt_Ack> {

		protected override async ETTask Run(Gamer gamer, Actor_GamerPrompt_Req request, Actor_GamerPrompt_Ack response) {
            // 再去看一遍：Root 根场景，是什么时候创建，并添加了哪些组件？
            Room room = Root.Instance.Scene.GetComponent<RoomComponent>().Get(gamer.RoomID);
            OrderControllerComponent orderController = room.GetComponent<OrderControllerComponent>();
            DeskCardsCacheComponent deskCardsCache = room.GetComponent<DeskCardsCacheComponent>();
            List<Card> handCards = new List<Card>(gamer.GetComponent<HandCardsComponent>().GetAll());
            CardsHelper.SortCards(handCards);
            if (gamer.UserID == orderController.Biggest) {
                response.Cards.AddRange(handCards.Where(card => card.CardWeight == handCards[handCards.Count - 1].CardWeight).ToArray());
            }
            else {
                List<IList<Card>> result = await CardsHelper.GetPrompt(handCards, deskCardsCache, deskCardsCache.Rule);
                if (result.Count > 0) {
                    response.Cards.AddRange(result[RandomHelper.RandomNumber(0, result.Count)]);
                }
            }
        }
	}
}
