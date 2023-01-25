namespace ET {

    namespace EventType {
        public struct EntryEvent1 {
        }   
        
        public struct EntryEvent2 {
        } 
        
        public struct EntryEvent3 {
        } 
    }

// 这是程序的固定入口吗 ?  不是   
    public static class Entry {
        public static void Init() {
        }

        public static void Start() {
            StartAsync().Coroutine();
        }
// 相关的初始化：Bson, ProtoBuf, Game.NetServices, Root etc
        private static async ETTask StartAsync() {
            WinPeriod.Init(); // Windows平台 Timer Tick的时间精度设置
            
            MongoHelper.Init();   // MongoDB 数据库的初始化: 这里像是没作什么工程，但涉及类相关所有静态变量的初始化  
            ProtobufHelper.Init();// 同上: 这个没有太细看，改天用到可以补上
            
            Game.AddSingleton<NetServices>(); // 网络连接初始化: 还没有理解透彻
            Game.AddSingleton<Root>();        // 它说是，管理场景根节点的，没看
            await Game.AddSingleton<ConfigComponent>().LoadAsync(); // Config组件会扫描所有的有ConfigAttribute标签的配置,加载进来
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent3());
        }
    }
}