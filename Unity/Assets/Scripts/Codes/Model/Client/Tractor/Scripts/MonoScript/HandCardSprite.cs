using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace ET.Server {
    // 【去找一下】：什么地方会需要用到这个类？去找这个类 HandCardSprite 是加在哪个场景的什么控件上
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

        public void OnClick(BaseEventData data) {
            float move = 50.0f;
            if (isSelect) {
                move = -move;
                Game.EventSystem.Run(Client.EventIdType.CancelHandCard, Poker);
            } else {
                Game.EventSystem.Run(Client.EventIdType.SelectHandCard, Poker);
            }
            RectTransform rectTransform = this.GetComponent<RectTransform>();
            rectTransform.anchoredPosition += Vector2.up * move;
            isSelect = !isSelect;
        }
    }
}
