﻿namespace ET {
    namespace EventType {
        public struct EntryEvent1 {
        }   
        public struct EntryEvent2 {
        } 
        public struct EntryEvent3 {
        } 
    }
    public static class Entry {
// 【空方法占位符】：在Program.cs 文件中说，如果不调用Model 域里哪怕是一个空方法，
		// 只要Model 域不被调用、没被引用，程序集就会被dotnet 项目裁剪掉。。。因为 DOTNET 项目，不曾引用 Model 程序域，所以不需要引入！！
        public static void Init() { 
        }
        public static void Start() {
            StartAsync().Coroutine();
        }
        // 【各种应用程序，第三方库等的初始化 】
        private static async ETTask StartAsync() {
            WinPeriod.Init(); // 早上：把这些忘记忘掉的，都再看一遍
            
            MongoHelper.Init();
            ProtobufHelper.Init();
            
            Game.AddSingleton<NetServices>(); // 早上：看这个
            Game.AddSingleton<Root>();
            await Game.AddSingleton<ConfigComponent>().LoadAsync();

            // 不知道：加这三个是在做什么？它没有起有意义的名字，但总之，它是事件，会触发相应的回调
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent3());
        }
    }
}