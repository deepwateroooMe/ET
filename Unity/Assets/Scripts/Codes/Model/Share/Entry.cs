namespace ET {

    namespace EventType {
        public struct EntryEvent1 {
        }   
        
        public struct EntryEvent2 {
        } 
        
        public struct EntryEvent3 {
        } 
    }
    
    public static class Entry {
        public static void Init() {
        }

        public static void Start() {
            StartAsync().Coroutine();
        }

        private static async ETTask StartAsync() {
            WinPeriod.Init(); // Windows平台 Timer Tick的时间精度设置
            
            MongoHelper.Init();   // MongoDB 数据库的初始化: 这里像是没作什么工程，但涉及类相关所有静态变量的初始化  
            ProtobufHelper.Init();// 同上
            
            Game.AddSingleton<NetServices>(); // 网络连接初始化
            Game.AddSingleton<Root>();
            await Game.AddSingleton<ConfigComponent>().LoadAsync();
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent3());
        }
    }
}