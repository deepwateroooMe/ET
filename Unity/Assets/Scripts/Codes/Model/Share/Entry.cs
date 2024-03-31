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
        public static void Init() { // 占位符，免得启动程序域 Model 被App 应用裁剪
        }
        public static void Start() {
            StartAsync().Coroutine();
        }
        private static async ETTask StartAsync() {
            WinPeriod.Init();
            MongoHelper.Init();   // 2 个空占位符方法调用：防止要用的第三方库，被项目裁剪
            ProtobufHelper.Init();
            Game.AddSingleton<NetServices>(); // Mode 程序域里：服务端的【网络模块】。去看Game: Game 是Unity 3 大主要回调的双端桥染实现静态类 
            Game.AddSingleton<Root>();
            await Game.AddSingleton<ConfigComponent>().LoadAsync();
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent3());
        }
    }
}