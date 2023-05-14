using System.Threading;
namespace ET {

    [ComponentOf(typeof(Scene))] // 是每个场景【SceneType?】：里都必须有的异步线程组件
    public class NetThreadComponent: Entity, IAwake, ILateUpdate, IDestroy {
        [StaticField]
        public static NetThreadComponent Instance;
        
        public Thread thread;
        public bool isStop;
    }
}