using System.Collections.Generic;
namespace ET.Server {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	// 为什么需要，区分这些类型呢？分门别类地管理之类的.
	// 如果不分门别类，位置服把它们都当同一类型，协程锁队列过长，影响效率，防碍了多线程多进程提效。分门别类后，位置服至少可以多进程减压、提速、提高反应灵敏性
	[UniqueId(0, 100)]
    public static class LocationType { // 几种不同的类型 
        public const int Unit = 0;
        public const int Player = 1;
        public const int Friend = 2;
        public const int Chat = 3;
        public const int Max = 100;
    }

	[ChildOf(typeof(LocationOneType))] // 这里 ChildOf, 它在热更域里，使用的时候，是真正添加为子控件 Component的
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