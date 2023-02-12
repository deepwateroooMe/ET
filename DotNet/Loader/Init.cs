using System;
using CommandLine;

namespace ET {
    public static class Init {

        public static void Start() {
            try {    // 捕捉各种异常
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                    Log.Error(e.ExceptionObject.ToString());
                };
                
                // 异步方法全部会回掉到主线程
                Game.AddSingleton<MainThreadSynchronizationContext>();
                // 命令行参数
                Parser.Default.ParseArguments<Options>(System.Environment.GetCommandLineArgs())
                    .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                    .WithParsed(Game.AddSingleton);
                
                Game.AddSingleton<TimeInfo>(); // 就是说，每个端，服务器端，或是每个客户端都各只有一个实例，所以不涉及任何的线程安全
// 下面是日志文件的具体的配置参数:　配置存放在配置文件里，同狠多其它配置文件一样，如数据库等
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
