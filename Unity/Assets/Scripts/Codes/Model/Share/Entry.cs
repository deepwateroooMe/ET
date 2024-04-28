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
            // 因为是CodeLoader==>Entry.Start() 来启动双端的某一端；双端任何一端程序启动、【静态方法、启动】它没任何其它办法，只能有这一种调用方式！
            // 【函数调用方式】调用协程：执行到 Game.AddSingleton<ConfigComponent>().LoadAsync(); 这行就返回。【LoadAsync() 不一定执行完了！】所以极可能报异常出错
            // ETTask 模块的 await正常调用： await StartAsync(); 一定执行到第一个 await call（）；  异步任务执行结束后、才返回
            StartAsync().Coroutine(); // LoadAsync() 不一定执行完了，就会返回【但是这里双端任何一端启动：返回与不返回，无实质本质区别】；
            // 调用上下文：是CodeLoader==>Entry.Start() 来启动双端的某一端
            // 上面，虽然是函数调用方式，但是下面的异步函数。。【不会报空或是出异常】：因为双端某端启动，这是【启动过程】，函数里要求的几大要件——一件一件去完成，不管用了多少桢。。。
            // 所以，不影响双端、任何一端的启动，也不会报空或是异常。Init.cs 加载了双端共享的必需要的组件
        }
		// 【TODO】：亲爱的表哥的活宝妹，觉得，双端启动时，是可能存在：【根场景的，启动加载、完成前、后】的回调之类的。检查这些双端根场景相关的细节！今天下午做这些
        // 上面，虽然是函数调用方式，但是下面的异步函数：还是最终、都会、四个定义要求的先后事件、全部按顺利执行完，不管用了多少桢。。。
        private static async ETTask StartAsync() {
            WinPeriod.Init();
            MongoHelper.Init();   // 2 个空占位符方法调用：防止要用的第三方库，被项目裁剪
            ProtobufHelper.Init();

            // Game 静态类：先把这里、某端的启动进程，想成一台物理机上的某个进程；静态类，就可以为本物理机任何进程可达可 access？？？这么想不对
            // 这里、某端的启动进程，想成一台物理机上的某个进程；静态类，就本进程随时可用可达
            Game.AddSingleton<NetServices>(); // Mode 程序域里：服务端的【网络模块】。去看Game: Game 是Unity 3 大主要回调的双端桥染实现静态类 
            Game.AddSingleton<Root>(); // 加了【端：双端任何一端、粒度单位：每个核每个进程】的根游戏场景

            // 接下来：按先后顺序，执行函数定义、要求的几大事件，每个调用，可能消耗多桢。。不管多少桢。。
			// 亲爱的表哥的活宝妹，今天上午，1 小时左右，就从这里，去看【客户端】的启动逻辑【TODO】：现在！！！
            await Game.AddSingleton<ConfigComponent>().LoadAsync(); // 【双端、任何一端】的配置、启动过程，大部分读懂了，仍有不少细节，要去翻框架确认细节！
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent1());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent2());
            await EventSystem.Instance.PublishAsync(Root.Instance.Scene, new EventType.EntryEvent3());
        }
    }
}