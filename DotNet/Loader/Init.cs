using System;
using CommandLine;
namespace ET {
    public static class Init {
        public static void Start() {
            try {    
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                    Log.Error(e.ExceptionObject.ToString());
                };
                
                // 异步方法全部会回掉到主线程
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
                Game.AddSingleton<CoroutineLockComponent>();
                
                ETTask.ExceptionHandler += Log.Error;
                
                Log.Console($"{Parser.Default.FormatCommandLine(Options.Instance)}");
                Game.AddSingleton<CodeLoader>().Start();
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
