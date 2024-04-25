using System.Collections.Generic;
namespace ET.Server {
    // 这个组件里，完整的组件逻辑，跟网络上读到的它人的理解相比，感觉还没能系统地边边角角都看明白：如服务端系统启动时，上报位置信息等细节
    // 读网页时读到，能够理解；可是自己读源码，就没能去想、和查这些细节
    // 自带超时检测机制：机制说，位置消息的发送者，近期，没活跃度，可能掉线了或是下线了？视为超时
    [ChildOf(typeof(ActorLocationSenderComponent))]
    public class ActorLocationSenderOneType: Entity, IAwake<int>, IDestroy {
        public const long TIMEOUT_TIME = 60 * 1000;
        public long CheckTimer;
        public int LocationType;
    }
    // 这个，可以再细看下    
    [ComponentOf(typeof(Scene))]
    public class ActorLocationSenderComponent: Entity, IAwake, IDestroy {
        public const long TIMEOUT_TIME = 60 * 1000;
        public static ActorLocationSenderComponent Instance { get; set; }
        public long CheckTimer;
        public ActorLocationSenderOneType[] ActorLocationSenderComponents = new ActorLocationSenderOneType[LocationType.Max];
    }
}
