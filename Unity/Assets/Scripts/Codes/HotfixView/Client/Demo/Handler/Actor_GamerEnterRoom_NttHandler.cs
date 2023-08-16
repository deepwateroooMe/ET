using UnityEngine;
using System.Collections.Generic;
namespace ET.Client {

// 系统标签化的消息处理器：这里实现玩家进房间的UI 视图控制变化等。
    [MessageHandler] 
    public class Actor_GamerEnterRoom_NttHandler : AMHandler<Actor_GamerEnterRoom_Ntt> {

        protected override async ETTask Run(ET.Session session, Actor_GamerEnterRoom_Ntt message) {
// 如果是房间里的第一个进入者，如何保证，这个房间一定是创建好了的？什么地方添加了这个UI 控件的生成，当然一定是客户端添加了这个UI 控件，LandlordsLobbyComponent.cs。
            UI uiRoom = session.DomainScene().GetComponent<UIComponent>().Get(UIType.TractorRoom); // 所以，到这里，当服务器端要去拿的时候，应该就不会为空了
            TractorRoomComponent landlordsRoomComponent = uiRoom.GetComponent<TractorRoomComponent>();
            GamerComponent gamerComponent = uiRoom.GetComponent<GamerComponent>();
            // 从匹配状态中切换为准备状态
            if (landlordsRoomComponent.Matching) {
                landlordsRoomComponent.Matching = false;
                GameObject matchPrompt = uiRoom.GameObject.Get<GameObject>("MatchPrompt");
                if (matchPrompt.activeSelf) {
                    matchPrompt.SetActive(false); // 【正在匹配中...】标签移除，失活
                    uiRoom.GameObject.Get<GameObject>("ReadyButton").SetActive(true); // 【准备】按钮激活，变为可点击状态：这里也去找一下这个按钮的处理逻辑
                }
            }
            int localGamerIndex = new List<GamerInfo>(message.Gamers).FindIndex(info => info.UserID == gamerComponent.LocalGamer.UserID);
            // 添加未显示玩家
            for (int i = 0; i < message.Gamers.Count; i++) {
                GamerInfo gamerInfo = message.Gamers[i];
                if (gamerInfo.UserID == 0)
                    continue;
                if (gamerComponent.Get(gamerInfo.UserID) == null) { // 如果添加的玩家在当前房间没有初始化完成，初始化并完成
                    Gamer gamer = GamerFactory.Create(gamerInfo.UserID, gamerInfo.IsReady); // 这个类：应该是需要重构掉的。
                    if ((localGamerIndex + 1) % 3 == i) {
                        // 玩家在本地玩家右边
                        landlordsRoomComponent.AddGamer(gamer, 2); // 实现的是：数据驱动UI 视图的更新变化
                    } else {
                        // 玩家在本地玩家左边
                        landlordsRoomComponent.AddGamer(gamer, 0);
                    }
                }
            }
            await ETTask.CompletedTask;
        }
    }
}