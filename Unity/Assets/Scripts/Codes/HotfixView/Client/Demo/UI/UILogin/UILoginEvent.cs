using System;
using UnityEngine;

namespace ET.Client {

    [UIEvent(UIType.UILogin)]
    public class UILoginEvent: AUIEvent { // 实现抽象类的两个回调方法：在创建  和  销毁  之后，分别需要做些什么  ？  是回调 

        public override async ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer) {
            await uiComponent.DomainScene().GetComponent<ResourcesLoaderComponent>().LoadAsync(UIType.UILogin.StringToAB());
            GameObject bundleGameObject = (GameObject) ResourcesComponent.Instance.GetAsset(UIType.UILogin.StringToAB(), UIType.UILogin);
            GameObject gameObject = UnityEngine.Object.Instantiate(bundleGameObject, UIEventComponent.Instance.GetLayer((int)uiLayer));
            UI ui = uiComponent.AddChild<UI, string, GameObject>(UIType.UILogin, gameObject);
            ui.AddComponent<UILoginComponent>();
            return ui;
        }

        public override void OnRemove(UIComponent uiComponent) {
            ResourcesComponent.Instance.UnloadBundle(UIType.UILogin.StringToAB());
        }
    }
}