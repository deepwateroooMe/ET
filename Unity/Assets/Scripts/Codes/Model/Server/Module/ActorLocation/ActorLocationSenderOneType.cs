using System.Collections.Generic;
namespace ET.Server {
    // 这个类：这个类，可以删除了。下午家里VS 里删除干净。那么同理的，我感觉 LocationOneTypeAwakeSystem.. 之类的应该也是自己瞎弄的，可是逻辑还没看完整，待检查 
    
    [ChildOf(typeof(ActorLocationSenderComponent))]
    public class ActorLocationSenderOneType: Entity, IAwake<int>, IDestroy {
        public const long TIMEOUT_TIME = 60 * 1000;
        public long CheckTimer;
        public int LocationType;
    }
}
