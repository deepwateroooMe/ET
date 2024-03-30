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
            WinPeriod.Init();
            MongoHelper.Init();
            ProtobufHelper.Init();
            Game.AddSingleton<NetServices>(); // Mode 程序域里：服务端的【网络模块】。去看Game
            Game.AddSingleton<Root>();
            await Game.AddSingleton<ConfigComponent>().LoadAsync();
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent3());
        }
    }
}