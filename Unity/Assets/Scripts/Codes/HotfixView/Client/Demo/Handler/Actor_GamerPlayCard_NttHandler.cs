using System;
using System.Collections.Generic;

namespace ET.Client {

    [MessageHandler]
    public class Actor_GamerPlayCard_NttHandler : AMHandler<Actor_GamerPlayCard_Ntt> {

        protected override async ETTask Run(ET.Session session, Actor_GamerPlayCard_Ntt message) {
            UI uiRoom = session.DomainScene().GetComponent<UIComponent>().Get(UIType.TractorRoom);
            GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
            Gamer gamer = gamerComponent.Get(message.UserID);
            if (gamer != null) {
                gamer.GetComponent<GamerUIComponent>().ResetPrompt();
                if (gamer.UserID == gamerComponent.LocalGamer.UserID) {
                    TractorInteractionComponent interaction = uiRoom.GetComponent<TractorRoomComponent>().Interaction;
                    interaction.Clear();
                    interaction.EndPlay();
                }
                HandCardsComponent handCards = gamer.GetComponent<HandCardsComponent>();
                handCards.PopCards(message.Cards);
                await ETTask.CompletedTask;
            }
        }
    }
}
