using System;
using System.Collections.Generic;
using ET;
namespace ET.Client {

    [MessageHandler(SceneType.Match)] // 地主先出牌：这个回调是，当设置了地主，广播了地主，那么这里回调：对于地主玩家，显示必要的UI, 激活他的出牌按钮，配置UI,等待地主出牌，把游戏推进下去
    public class Actor_AuthorityPlayCard_NttHandler : AMHandler<Actor_AuthorityPlayCard_Ntt> {

        protected override void Run(ETModel.Session session, Actor_AuthorityPlayCard_Ntt message) {
            UI uiRoom = Game.Scene.GetComponent<UIComponent>().Get(UIType.TractorRoom);
            GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
            Gamer gamer = gamerComponent.Get(message.UserID);
            if (gamer != null) {
                // 重置玩家提示
                gamer.GetComponent<GamerUIComponent>().ResetPrompt();
                // 当玩家为先手，清空出牌
                if (message.IsFirst) {
                    gamer.GetComponent<HandCardsComponent>().ClearPlayCards();
                }
                // 显示出牌按钮
                if (gamer.UserID == gamerComponent.LocalGamer.UserID) {
                    TractorInteractionComponent interaction = uiRoom.GetComponent<TractorRoomComponent>().Interaction;
                    interaction.IsFirst = message.IsFirst;
                    interaction.StartPlay();
                }
            }
        }
    }
}
