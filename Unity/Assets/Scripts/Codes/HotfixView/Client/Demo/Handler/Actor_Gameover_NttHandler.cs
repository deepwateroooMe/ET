using System;
using System.Collections.Generic;
using ETModel;
namespace ET.Client {

    [MessageHandler]
    public class Actor_Gameover_NttHandler : AMHandler<Actor_Gameover_Ntt> {
        protected override void Run(ET.Session session, Actor_Gameover_Ntt message) {
            UI uiRoom = Game.Scene.GetComponent<UIComponent>().Get(UIType.TractorRoom);
            GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
            Identity localGamerIdentity = gamerComponent.LocalGamer.GetComponent<HandCardsComponent>().AccessIdentity;
            UI uiEndPanel = TractorEndFactory.Create(UIType.TractorEnd, uiRoom, (Identity)message.Winner == localGamerIdentity);
            TractorEndComponent landlordsEndComponent = uiEndPanel.GetComponent<TractorEndComponent>();
            foreach (GamerScore gamerScore in message.GamersScore) {
                Gamer gamer = uiRoom.GetComponent<GamerComponent>().Get(gamerScore.UserID);
                gamer.GetComponent<GamerUIComponent>().UpdatePanel();
                gamer.GetComponent<HandCardsComponent>().Hide();
                landlordsEndComponent.CreateGamerContent(
                    gamer,
                    (Identity)message.Winner,
                    message.BasePointPerMatch,
                    message.Multiples,
                    gamerScore.Score);
            }
            TractorRoomComponent landlordsRoomComponent = uiRoom.GetComponent<TractorRoomComponent>();
            landlordsRoomComponent.Interaction.Gameover();
            landlordsRoomComponent.ResetMultiples();
        }
    }
}
