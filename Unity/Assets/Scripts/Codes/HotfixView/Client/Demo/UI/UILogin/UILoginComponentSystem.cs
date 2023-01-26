using UnityEngine;
using UnityEngine.UI;

namespace ET.Client {

    [FriendOf(typeof(UILoginComponent))]
    public static class UILoginComponentSystem {

        [ObjectSystem]
        public class UILoginComponentAwakeSystem : AwakeSystem<UILoginComponent> {

            protected override void Awake(UILoginComponent self) {
                // 去查一下:  为什么UI上总要背这个 什么鬼东西收集器 ? 它收集了这个当前类定义单位下的所有UI元件,可以通过它拿到某个某些UI控件,以便添加回调事件等
                ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
                self.loginBtn = rc.Get<GameObject>("LoginBtn");
                
                self.loginBtn.GetComponent<Button>().onClick.AddListener(()=> { self.OnLogin(); });
                self.account = rc.Get<GameObject>("Account");
                self.password = rc.Get<GameObject>("Password");
            }
        }
        
        public static void OnLogin(this UILoginComponent self) { // <<<<<<<<<<<<<<<<<<<< 可以从这里去查看一下客户端的登录流程
            LoginHelper.Login(
                self.DomainScene(), 
                self.account.GetComponent<InputField>().text, 
                self.password.GetComponent<InputField>().text).Coroutine();
        }
    }
}
