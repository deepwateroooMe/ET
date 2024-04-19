using System.Collections.Generic;
namespace ET.Server {

// 为什么需要，区分这些类型呢？分门别类地管理之类的【TODO】：去查细节
	[UniqueId(0, 100)]
    public static class LocationType { 
        public const int Unit = 0;
        public const int Player = 1;
        public const int Friend = 2;
        public const int Chat = 3;
        public const int Max = 100;
    }

	[ChildOf(typeof(LocationOneType))]
    public class LockInfo: Entity, IAwake<long, CoroutineLock>, IDestroy {
        public long LockInstanceId; // 锁的是：【被查询位置的、被查目标】的实例号
        public CoroutineLock CoroutineLock;
    }
    [ChildOf(typeof(LocationManagerComoponent))]
    public class LocationOneType: Entity, IAwake<int> { // 每种位置类型、的管理
        public int LocationType;
        public readonly Dictionary<long, long> locations = new Dictionary<long, long>();
        public readonly Dictionary<long, LockInfo> lockInfos = new Dictionary<long, LockInfo>();
    }
    [ComponentOf(typeof(Scene))]
    public class LocationManagerComoponent: Entity, IAwake { // 位置服总管：几种不同类型，同数组的
        public LocationOneType[] LocationOneTypes = new LocationOneType[LocationType.Max];
    }
}