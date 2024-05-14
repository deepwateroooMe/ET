using MemoryPack;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
namespace ET {

	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
    public struct EntryEvent1 {}   
    public struct EntryEvent2 {} 
    public struct EntryEvent3 {}

    public static class Entry {
        public static void Init() {
        }
        public static void Start() {
            StartAsync().Coroutine();
        }

        private static async ETTask StartAsync() {
            WinPeriod.Init();
            // 注册Mongo type
            MongoRegister.Init();
            // 注册Entity序列化器
            EntitySerializeRegister.Init();
            World.Instance.AddSingleton<IdGenerater>();
            World.Instance.AddSingleton<OpcodeType>();
            World.Instance.AddSingleton<ObjectPool>();
            World.Instance.AddSingleton<MessageQueue>();
            World.Instance.AddSingleton<NetServices>();
            World.Instance.AddSingleton<NavmeshComponent>();
            World.Instance.AddSingleton<LogMsg>();
            // 创建需要reload的code singleton
            CodeTypes.Instance.CreateCode();
            await World.Instance.AddSingleton<ConfigLoader>().LoadAsync();

			// 【双端】纤程这里，一个创建Invoke 事件，就把【主线程纤程 Main】加载起来了
            await FiberManager.Instance.Create(SchedulerType.Main, ConstFiberId.Main, 0, SceneType.Main, "");
        }
    }
}