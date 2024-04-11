using System;
using CommandLine;
namespace ET {
	// CodeLoader 前：加载必备的运行、最基部件部分，否则任何一端都无法正常启动
	// 看：哪些双端共享最基部件、组件，可以这里加载的？
    public static class Init {
        public static void Start() {
            try {    
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                    Log.Error(e.ExceptionObject.ToString());
                };
                
                // 异步方法全部会回掉到主线程: 这个看得没问题，可以进阶、试看ET8 多线程多进程了
                Game.AddSingleton<MainThreadSynchronizationContext>();
                // 命令行参数
                Parser.Default.ParseArguments<Options>(System.Environment.GetCommandLineArgs())
                    .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                    .WithParsed(Game.AddSingleton); // Parsed 完成后的回调：执行Game.AddSingleton<刚才的结果>
				// CodeLoader 帮助程序域：主要是想要：【服务端】与【客户端】的加载一致。与服务端是一样的
                Game.AddSingleton<TimeInfo>();
                Game.AddSingleton<Logger>().ILog = new NLogger(Options.Instance.AppType.ToString(), Options.Instance.Process, "../Config/NLog/NLog.config");
                Game.AddSingleton<ObjectPool>();
                Game.AddSingleton<IdGenerater>();
                Game.AddSingleton<EventSystem>();
                Game.AddSingleton<TimerComponent>();
				// 下面【协程锁】：就把ETTask 的封装，连带、全部加载进双端了、即时可用
                Game.AddSingleton<CoroutineLockComponent>(); // 现在再看【协程锁】：也狠简单没难度！
                
                ETTask.ExceptionHandler += Log.Error;
                
                Log.Console($"{Parser.Default.FormatCommandLine(Options.Instance)}");
                Game.AddSingleton<CodeLoader>().Start(); // <<<<<<<<<<<<<<<<<<<< 双端、共享的一致启动方式，借助CodeLoader 项目
            }
            catch (Exception e) {
                Log.Error(e);
            }
        }
        public static void Update() {
            Game.Update();
        }
        public static void LateUpdate() {
            Game.LateUpdate();
        }
        public static void FrameFinishUpdate() {
            Game.FrameFinishUpdate();
        }
    }
}
