using System.Collections.Generic;
namespace ET.Server {
    // 这个【ActorLocation】文件夹：原本只是没有ActorLocationSenderOneType.cs 类。不知道添加是为什么 
    [ChildOf(typeof(LocationComponent))]
    public class LockInfo: Entity, IAwake<long, CoroutineLock>, IDestroy {
        public long LockInstanceId;
        public CoroutineLock CoroutineLock;
    }
    [ComponentOf(typeof(Scene))]
    public class LocationComponent: Entity, IAwake {
        public readonly Dictionary<long, long> locations = new Dictionary<long, long>();
        public readonly Dictionary<long, LockInfo> lockInfos = new Dictionary<long, LockInfo>();
    }
}