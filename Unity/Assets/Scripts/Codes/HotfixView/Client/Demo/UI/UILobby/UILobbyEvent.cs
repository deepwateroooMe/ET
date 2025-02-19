﻿using UnityEngine;
namespace ET.Client {
    // UI 系统的事件机制：接收到 LoginFinish 之后触发的大厅创建
    [UIEvent(UIType.UILobby)]
    public class UILobbyEvent: AUIEvent {

        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer) {
            await ETTask.CompletedTask;
            await uiComponent.DomainScene().GetComponent<ResourcesLoaderComponent>().LoadAsync(UIType.UILobby.StringToAB());
            GameObject bundleGameObject = (GameObject) ResourcesComponent.Instance.GetAsset(UIType.UILobby.StringToAB(), UIType.UILobby);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, UIEventComponent.Instance.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILobby, gameObject);
            ui.AddComponent<UILobbyComponent>();
            return ui;
        }
        public override void OnRemove(UIComponent uiComponent) {
            ResourcesComponent.Instance.UnloadBundle(UIType.UILobby.StringToAB());
        }
    }
}