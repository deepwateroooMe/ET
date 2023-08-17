using System;
using System.Collections.Generic;
using ET;
namespace ET.Client {

    [MessageHandler] // 这个标签，不带场景的：算是标注客户端处理逻辑，UI 同步跟上
    public class Actor_GamerDontPlay_NttHandler : AMHandler<Actor_GamerDontPlay_Ntt> {
        protected override async ETTask Run(ET.Session session, Actor_GamerDontPlay_Ntt message) {
            UI uiRoom = session.DomainScene().GetComponent<UIComponent>().Get(UIType.TractorRoom);

            GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
            // Gamer gamer = gamerComponent.Get(message.UserID);
            Gamer gamer = GamerComponentSystem.Get(uiRoom.GetComponent<GamerComponent>(), message.UserID);
                                                   
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
