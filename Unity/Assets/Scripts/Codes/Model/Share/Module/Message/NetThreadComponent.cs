using System.Threading;
namespace ET {
    // 【NetThreadComponent 组件】：网络交互的底层原理不懂。没有生成系，只有一个【NetInnerComponentSystem】。外网组件找不见
    // 这个模块：感觉就是模块，自顶向下，异步网络调用的传递方向等，弄不懂；或底层信道上发消息两端的底层回调，不懂！
// 是每个场景【SceneType?】：里都必须有的异步线程组件. 场景 Scene, 与场景类型SceneType
    [ComponentOf(typeof(Scene))] 
    public class NetThreadComponent: Entity, IAwake, ILateUpdate, IDestroy {
        [StaticField]
        public static NetThreadComponent Instance; // 单例
        public Thread thread;
        public bool isStop;
    }
}