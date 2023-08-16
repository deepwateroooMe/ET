using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ET.Client {

    [MessageHandler]
    public class Actor_GameStart_NttHandler : AMHandler<Actor_GameStart_Ntt> {

        protected override async ETTask Run(Session session, Actor_GameStart_Ntt message) {
            UI uiRoom = session.DomainScene().GetComponent<UIComponent>().Get(UIType.TractorRoom);
            GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
            // 初始化玩家UI: 这里不知道为什么会有多个不同消息
            foreach (GamerCardNum gamerCardNum in message.GamersCardNum) {
                Gamer gamer = uiRoom.GetComponent<GamerComponent>().Get(gamerCardNum.UserID);
                GamerUIComponent gamerUI = gamer.GetComponent<GamerUIComponent>();
                gamerUI.GameStart(); // 更新玩家UI, 身上背的小面板
                HandCardsComponent handCards = gamer.GetComponent<HandCardsComponent>();
                if (handCards != null) {
                    handCards.Reset();
                }
                else {
                    handCards = gamer.AddComponent<HandCardsComponent, GameObject>(gamerUI.Panel);
                }
                handCards.Appear();
                if (gamer.UserID == gamerComponent.LocalGamer.UserID) {
                    // 本地玩家添加手牌
                    handCards.AddCards(message.HandCards);
                }
                else {
                    // 设置其他玩家手牌数
                    handCards.SetHandCardsNum(gamerCardNum.Num);
                }
            }
            // 显示牌桌UI
            GameObject desk = uiRoom.GameObject.Get<GameObject>("Desk");
            desk.SetActive(true);
            GameObject lordPokers = desk.Get<GameObject>("LordPokers");
            // 重置地主牌
            Sprite lordSprite = CardHelper.GetCardSprite("None");
            for (int i = 0; i < lordPokers.transform.childCount; i++) {
                lordPokers.transform.GetChild(i).GetComponent<Image>().sprite = lordSprite;
            }
            TractorRoomComponent uiRoomComponent = uiRoom.GetComponent<TractorRoomComponent>();
            // 清空选中牌
            uiRoomComponent.Interaction.Clear();
            // 设置初始倍率
            uiRoomComponent.SetMultiples(1);

            await ETTask.CompletedTask;
        }
    }
}
