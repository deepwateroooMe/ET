using System;
using System.Collections.Generic;
using ET;
using UnityEngine;
namespace ET.Client {

    [MessageHandler]
    public class Actor_GamerReady_NttHandler : AMHandler<Actor_GamerReady_Ntt> {
        protected override async ETTask Run(ET.Session session, Actor_GamerReady_Ntt message) {
            UI uiRoom = session.DomainScene().GetComponent<UIComponent>().Get(UIType.TractorRoom);
            GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
            Gamer gamer = gamerComponent.Get(message.UserID);
            gamer.GetComponent<GamerUIComponent>().SetReady();
            // 本地玩家准备,隐藏准备按钮
            if (gamer.UserID == gamerComponent.LocalGamer.UserID) {
                uiRoom.GameObject.Get<GameObject>("ReadyButton").SetActive(false);
            }
            await ETTask.CompletedTask;
        }
    }
}
