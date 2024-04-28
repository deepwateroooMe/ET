using UnityEngine;
namespace ET.Client {
	// 【客户端】：对应的是，Unity 下的 Root 根场景下的、几层组件。
	// 框架提取出来成组件，应该是方便，晚点儿、更底层的控件、更直接快速地 access 这些祖先级控件。。
    [ObjectSystem]
    public class GlobalComponentAwakeSystem: AwakeSystem<GlobalComponent> {
        protected override void Awake(GlobalComponent self) {
            GlobalComponent.Instance = self;
            self.Global = GameObject.Find("/Global").transform;
            self.Unit = GameObject.Find("/Global/Unit").transform;
            self.UI = GameObject.Find("/Global/UI").transform;
        }
    }
}