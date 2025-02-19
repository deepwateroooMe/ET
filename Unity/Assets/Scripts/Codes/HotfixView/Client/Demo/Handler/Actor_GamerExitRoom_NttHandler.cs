﻿using ET;
namespace ET.Client {

    [MessageHandler(SceneType.Map)]  // 不知道写哪个服？再改
    public class Actor_GamerExitRoom_NttHandler : AMHandler<Actor_GamerExitRoom_Ntt> {
        protected override async ETTask Run(ET.Session session, Actor_GamerExitRoom_Ntt message) {
            UI uiRoom = session.DomainScene().GetComponent<UIComponent>().Get(UIType.TractorRoom);
            TractorRoomComponent landlordsRoomComponent = uiRoom.GetComponent<TractorRoomComponent>();
            landlordsRoomComponent.RemoveGamer(message.UserID);
            await ETTask.CompletedTask;
        }
    }
}
