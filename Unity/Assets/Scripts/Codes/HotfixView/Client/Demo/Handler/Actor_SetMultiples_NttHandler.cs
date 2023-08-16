using System;
using System.Collections.Generic;
using ET;

namespace ET.Client {
    [MessageHandler]
    public class Actor_SetMultiples_NttHandler : AMHandler<Actor_SetMultiples_Ntt> {

        protected override async ETTask Run(ET.Session session, Actor_SetMultiples_Ntt message) {
            UI uiRoom = session.DomainScene().GetComponent<UIComponent>().Get(UIType.TractorRoom);
            uiRoom.GetComponent<TractorRoomComponent>().SetMultiples(message.Multiples);
            await ETTask.CompletedTask;
        }
    }
}
