using System.Threading;
namespace ET {
    // 【NetThreadComponent 组件】：网络交互的底层原理不懂。没有生成系，只有一个【NetInnerComponentSystem】。外网组件找不见，不知道是为什么
// 是每个场景【SceneType?】：里都必须有的异步线程组件. 场景 Scene, 与场景类型SceneType
    [ComponentOf(typeof(Scene))] 
    public class NetThreadComponent: Entity, IAwake, ILateUpdate, IDestroy {
        [StaticField]
        public static NetThreadComponent Instance;
        
        public Thread thread;
        public bool isStop;
    }
}