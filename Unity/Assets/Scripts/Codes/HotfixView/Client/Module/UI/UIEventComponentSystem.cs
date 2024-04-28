using System;
using System.Collections.Generic;
using UnityEngine;
namespace ET.Client {
    // 管理所有UI GameObject 以及UI事件
    [FriendOf(typeof(UIEventComponent))]
    public static class UIEventComponentSystem {
        [ObjectSystem]
        public class UIEventComponentAwakeSystem : AwakeSystem<UIEventComponent> {
            protected override void Awake(UIEventComponent self) {
                UIEventComponent.Instance = self;
                GameObject uiRoot = GameObject.Find("/Global/UI");
                ReferenceCollector referenceCollector = uiRoot.GetComponent<ReferenceCollector>(); // 细看了一遍，包括Editor 配置；基本都懂【拖拽功能】没看
				// 这里，referenceCollector, 就充当了【Unity 客户端、得以实现热更新】的桥梁，帮导出 GameObject
                self.UILayers.Add((int)UILayer.Hidden, referenceCollector.Get<GameObject>(UILayer.Hidden.ToString()).transform);
                self.UILayers.Add((int)UILayer.Low, referenceCollector.Get<GameObject>(UILayer.Low.ToString()).transform);
                self.UILayers.Add((int)UILayer.Mid, referenceCollector.Get<GameObject>(UILayer.Mid.ToString()).transform);
                self.UILayers.Add((int)UILayer.High, referenceCollector.Get<GameObject>(UILayer.High.ToString()).transform);
                var uiEvents = EventSystem.Instance.GetTypes(typeof (UIEventAttribute));
                foreach (Type type in uiEvents) {
                    object[] attrs = type.GetCustomAttributes(typeof(UIEventAttribute), false);
                    if (attrs.Length == 0)
                    {
                        continue;
                    }
                    UIEventAttribute uiEventAttribute = attrs[0] as UIEventAttribute;
                    AUIEvent aUIEvent = Activator.CreateInstance(type) as AUIEvent;
                    self.UIEvents.Add(uiEventAttribute.UIType, aUIEvent);
                }
            }
        }
        public static async ETTask<UI> OnCreate(this UIEventComponent self, UIComponent uiComponent, string uiType, UILayer uiLayer) {
            try {
                UI ui = await self.UIEvents[uiType].OnCreate(uiComponent, uiLayer);
                return ui;
            }
            catch (Exception e) {
                throw new Exception($"on create ui error: {uiType}", e);
            }
        }
        public static Transform GetLayer(this UIEventComponent self, int layer) {
            return self.UILayers[layer];
        }
        public static void OnRemove(this UIEventComponent self, UIComponent uiComponent, string uiType) {
            try {
                self.UIEvents[uiType].OnRemove(uiComponent);
            }
            catch (Exception e) {
                throw new Exception($"on remove ui error: {uiType}", e);
            }
        }
    }
}