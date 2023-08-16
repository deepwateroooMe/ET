using System;
using System.Collections.Generic;
using ET;
namespace ET.Client {

    [MessageHandler]
    public class Actor_GamerDontPlay_NttHandler : AMHandler<Actor_GamerDontPlay_Ntt> {
        protected override async ETTask Run(ET.Session session, Actor_GamerDontPlay_Ntt message) {
            UI uiRoom = session.DomainScene().GetComponent<UIComponent>().Get(UIType.TractorRoom);
            GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
            Gamer gamer = gamerComponent.Get(message.UserID);
            if (gamer != null) {
                if (gamer.UserID == gamerComponent.LocalGamer.UserID) {
                    uiRoom.GetComponent<TractorRoomComponent>().Interaction.EndPlay();
                }
                gamer.GetComponent<HandCardsComponent>().ClearPlayCards();
                gamer.GetComponent<GamerUIComponent>().SetDiscard();
            }
            await ETTask.CompletedTask;
        }
    }
}
