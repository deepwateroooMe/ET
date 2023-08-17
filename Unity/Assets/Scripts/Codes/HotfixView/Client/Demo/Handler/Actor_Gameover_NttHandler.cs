using System;
using System.Collections.Generic;
namespace ET { // 原本说这里是客户端，我把它改成了双端

    // 【亲爱的表哥的活宝妹现在狠强大，这些编译错误也就是分分钟秒秒钟解决掉的事儿！！爱表哥，爱生活！！！】
    [MessageHandler] // 这里说，这个标签没有提供，【SceneType.XXX】场景服参数！！！是哪个小服在处理这块儿逻辑呢？把这个先放一下，对【参考项目】不太熟悉
    public class Actor_Gameover_NttHandler : AMHandler<Actor_Gameover_Ntt> {
        protected override async ETTask Run(Session session, Actor_Gameover_Ntt message) {
            // UI uiRoom = Game.Scene.GetComponent<UIComponent>().Get(UIType.TractorRoom);
            UI uiRoom = session.DomainScene().GetComponent<UIComponent>().Get(UIType.TractorRoom);
            GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
            Identity localGamerIdentity = gamerComponent.LocalGamer.GetComponent<HandCardsComponent>().AccessIdentity;
            UI uiEndPanel = TractorEndFactory.Create(UIType.TractorEnd, uiRoom, (Identity)message.Winner == localGamerIdentity);
            TractorEndComponent landlordsEndComponent = uiEndPanel.GetComponent<TractorEndComponent>();
            foreach (GamerScore gamerScore in message.GamersScore) {
                // Gamer gamer = w.Get(gamerScore.UserID);
                Gamer gamer = GamerComponentSystem.Get(uiRoom.GetComponent<GamerComponent>(), gamerScore.UserID);
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
            await ETTask.CompletedTask;
        }
    }
}