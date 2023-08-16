using System;
using System.Collections.Generic;
using ET;
namespace ET.Client {
    // 这里注意这个文件的地址程序域【HotfixView】：感觉是被亲爱的表哥的活宝妹给弄放错地方了？Model 与 Hotfix 相对清楚，但涉及视图层的，活宝妹还没能理解透彻。。爱表哥，爱生活！！！
    [MessageHandler(SceneType.Match)] // 地主先出牌：这个回调是，当设置了地主，广播了地主，那么这里回调：对于地主玩家，显示必要的UI, 激活他的出牌按钮，配置UI,等待地主出牌，把游戏推进下去
    public class Actor_AuthorityPlayCard_NttHandler : AMHandler<Actor_AuthorityPlayCard_Ntt> {

        protected override ETTask Run(Session session, Actor_AuthorityPlayCard_Ntt message) {
            // UI uiRoom = Game.Scene.GetComponent<UIComponent>().Get(UIType.TractorRoom); // 这里，框架里再找一下，是哪里添加了这个组件？是【客户端】场景添加了这个组件。如何去拿客户端场景？
            UI uiRoom = session.DomainScene().GetComponent<UIComponent>().Get(UIType.TractorRoom); // 这里，框架里再找一下，是哪里添加了这个组件？是【客户端】场景添加了这个组件。如何去拿客户端场景？
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
            return await ETTask.CompletedTask;
        }
    }
}