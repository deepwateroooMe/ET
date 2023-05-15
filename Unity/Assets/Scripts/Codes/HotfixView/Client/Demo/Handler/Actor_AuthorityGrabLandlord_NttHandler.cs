using System;
using System.Collections.Generic;
using ET;
namespace ET.Client {

    [MessageHandler(SceneType.Match)] // 要添加【匹配服】吗？
    public class Actor_AuthorityGrabLandlord_NttHandler : AMHandler<Actor_AuthorityGrabLandlord_Ntt> {

        protected override void Run(ET.Session session, Actor_AuthorityGrabLandlord_Ntt message) {
            UI uiRoom = Game.Scene.GetComponent<UIComponent>().Get(UIType.TractorRoom);
            GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
            if (message.UserID == gamerComponent.LocalGamer.UserID) {
                // 显示抢地主交互
                uiRoom.GetComponent<TractorRoomComponent>().Interaction.StartGrab();
            }
        }
    }
}
