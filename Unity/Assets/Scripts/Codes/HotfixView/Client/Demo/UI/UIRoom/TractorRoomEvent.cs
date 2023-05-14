using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace ET.Client {

    // UI 系统的事件机制：定义，如何创建拖拉机游戏房间【TODO:】UNITY 里是需要制作相应预设的
    [UIEvent(UIType.TractorRoom)]
    public class TractorRoomEvent: AUIEvent {

        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer) {
            await ETTask.CompletedTask;
            await uiComponent.DomainScene().GetComponent<ResourcesLoaderComponent>().LoadAsync(UIType.TractorRoom.StringToAB());
            GameObject bundleGameObject = (GameObject) ResourcesComponent.Instance.GetAsset(UIType.TractorRoom.StringToAB(), UIType.TractorRoom);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, UIEventComponent.Instance.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.TractorRoom, gameObject);
            ui.AddComponent<TractorRoomComponent>(); // <<<<<<<<<<<<<<<<<<<< 
            return ui;
        }
        public override void OnRemove(UIComponent uiComponent) {
            ResourcesComponent.Instance.UnloadBundle(UIType.TractorRoom.StringToAB());
        }
    }
}