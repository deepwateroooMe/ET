using UnityEngine;

namespace ET.Client {

    // 把UILogin组件化：到这一个组件中来
    [ComponentOf(typeof(UI))]
    public class UILoginComponent: Entity, IAwake {
        public GameObject account;
        public GameObject password;
        public GameObject loginBtn; // 那么再去找： 点击回调是在哪里设置的  ？
    }
}
