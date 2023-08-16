using System;
using System.Collections.Generic;
namespace ET.Client {

    [MessageHandler]
    public class Actor_GamerGrabLandlordSelect_NttHandler : AMHandler<Actor_GamerGrabLandlordSelect_Ntt> {
        protected override async ETTask Run(ET.Session session, Actor_GamerGrabLandlordSelect_Ntt message) {
            UI uiRoom = session.DomainScene().GetComponent<UIComponent>().Get(UIType.TractorRoom);
            GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
            Gamer gamer = gamerComponent.Get(message.UserID);
            if (gamer != null) {
                GamerUIComponent gamerUIComponent = gamer.GetComponent<GamerUIComponent>();
                if (gamer.UserID == gamerComponent.LocalGamer.UserID) {
                    uiRoom.GetComponent<TractorRoomComponent>().Interaction.EndGrab();
                }
                if (message.IsGrab) {
                    gamerUIComponent.SetGrab(GrabLandlordState.Grab);
                }
                else {
                    gamerUIComponent.SetGrab(GrabLandlordState.UnGrab);
                }
            }
            await ETTask.CompletedTask;
        }
    }
}
