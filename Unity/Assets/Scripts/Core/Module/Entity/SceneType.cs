using System;
namespace ET {
    [Flags]
    public enum SceneType: ulong {
        None = 0,
        Process = 1,
        Manager = 1 << 2,
        Realm = 1 << 3,
        Gate = 1 << 4,
        Http = 1 << 5,
        Location = 1 << 6,
        Map = 1 << 7,
        Router = 1 << 8,        // 【场景】：可以是，路由器场景 
        RouterManager = 1 << 9, // 【场景】：可以是，路由器 Manager场景 
        Robot = 1 << 10,
        BenchmarkClient = 1 << 11,
        BenchmarkServer = 1 << 12,
        Benchmark = 1 << 13,
        // 客户端Model层
        Client = 1 << 30,
        Current = 1ul << 31,
        All = ulong.MaxValue,
    }
    public static class SceneTypeHelper {
        public static bool HasSameFlag(this SceneType a, SceneType b) {
			// 上面的 enum: 每个场景，都只有一个1 位，判断是否完全相同，是否为同一个场景而已
            if (((ulong) a & (ulong) b) == 0) {
                return false;
            }
            return true;
        }
    }
}