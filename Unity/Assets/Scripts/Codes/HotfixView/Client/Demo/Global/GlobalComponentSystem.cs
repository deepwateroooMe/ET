using UnityEngine;

namespace ET.Client {

    [ObjectSystem]
    public class GlobalComponentAwakeSystem: AwakeSystem<GlobalComponent> {

        protected override void Awake(GlobalComponent self) {
            GlobalComponent.Instance = self;
            
            self.Global = GameObject.Find("/Global").transform;
            self.Unit = GameObject.Find("/Global/Unit").transform; // Unit: 它是框架中组件的最小单位吗? 这里,它是场景中 Global组件下的一个子控件,UI视图中可以看见
            self.UI = GameObject.Find("/Global/UI").transform;
        }
    }
}