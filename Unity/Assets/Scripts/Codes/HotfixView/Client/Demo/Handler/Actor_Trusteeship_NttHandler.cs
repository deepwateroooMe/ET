using System;
using System.Collections.Generic;
using System.Linq;
using ET;
namespace ET.Client {
    [MessageHandler]
    public class Actor_Trusteeship_NttHandler : AMHandler<Actor_Trusteeship_Ntt> {

        protected override async ETTask Run(Session session, Actor_Trusteeship_Ntt message) {
            UI uiRoom = session.DomainScene().GetComponent<UIComponent>().Get(UIType.TractorRoom);
            GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
            Gamer gamer = gamerComponent.Get(message.UserID);
            if (gamer.UserID == ClientComponent.Instance.LocalPlayer.UserID) {
                TractorInteractionComponent interaction = uiRoom.GetComponent<TractorRoomComponent>().Interaction;
                interaction.isTrusteeship = message.IsTrusteeship;
            }
            await ETTask.CompletedTask; // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
        }
    }
}
