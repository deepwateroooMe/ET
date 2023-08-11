using System.Collections.Generic;
namespace ET.Server {
    // 感觉这些模块，还是被自己弄得乱七八糟。。。先放一下
    
    // 分配房间服务器组件，逻辑在AllotMapComponentSystem扩展
    [ComponentOf(typeof(Scene))]
    public class AllotMapComponent : Entity, IAwake, IStart {
        // 关键是，这个地图服，不是重构后的框架里什么地方狠好拿吗，就不需要这个组件了呀。。。StartSceneConfigCategory 里面添加一下地图服就可以了
        public readonly List<StartSceneConfig> MapAddress = new List<StartSceneConfig>();
    }
}