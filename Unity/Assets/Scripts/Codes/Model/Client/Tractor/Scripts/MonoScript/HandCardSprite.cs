using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace ET.Client {
    // 【去找一下】：什么地方会需要用到这个类？去找这个类 HandCardSprite 是加在手牌还是什么地方的一个UI 场景中的脚本
    public class HandCardSprite : MonoBehaviour {
        public Card Poker { get; set; }
        private bool isSelect;

        void Start() {
            EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers = new List<EventTrigger.Entry>();
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback = new EventTrigger.TriggerEvent();
            clickEntry.callback.AddListener(new UnityAction<BaseEventData>(OnClick));
            eventTrigger.triggers.Add(clickEntry);
        }
        // 两类事件的作用是说：玩家可以选择要出的牌，也可以取消选过、分前打算出的牌
        public void OnClick(BaseEventData data) {
            float move = 50.0f;
            if (isSelect) {
                move = -move;
                // 【客户端】：借助Game.cs 这个桥，把Model 层这个类，与客户端热更域？的逻辑连通起来
                Game.EventSystem.Run(Client.EventIdType.CancelHandCard, Poker); // 取消选牌，会重新选牌
            } else {
                Game.EventSystem.Run(Client.EventIdType.SelectHandCard, Poker); // 选牌
            }
            RectTransform rectTransform = this.GetComponent<RectTransform>();
            rectTransform.anchoredPosition += Vector2.up * move;
            isSelect = !isSelect;
        }
    }
}
