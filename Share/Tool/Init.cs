using System;
using System.Collections.Generic;
using System.Threading;
using CommandLine;
namespace ET.Server { // 弄出了三个 Init 同名文件，【服务端】【客户端】和【双端】。双端的时候，是服务于【双端】模式启动
    // 【服务器】端的起始程序: 这个服务端的启动日志追踪分析，亲爱的表哥的活宝妹弄过，基本原理明白。只看还不太懂的细节
    internal static class Init {

        private static int Main(string[] args) {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                Log.Error(e.ExceptionObject.ToString());
            };
            
            try {
                // 异步方法全部会回掉到主线程
                Game.AddSingleton<MainThreadSynchronizationContext>();
                
                // 命令行参数
                Parser.Default.ParseArguments<Options>(args)
                    .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                    .WithParsed(Game.AddSingleton);
                
                Game.AddSingleton<TimeInfo>();
                Game.AddSingleton<Logger>().ILog = new NLogger(Options.Instance.AppType.ToString(), Options.Instance.Process, "../Config/NLog/NLog.config");
                Game.AddSingleton<ObjectPool>();
                Game.AddSingleton<IdGenerater>();
                Game.AddSingleton<EventSystem>();
                Game.AddSingleton<Root>();
                
                ETTask.ExceptionHandler += Log.Error;
                Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly);
                EventSystem.Instance.Add(types);
                MongoHelper.Init();
                ProtobufHelper.Init();
                
                Log.Info($"server start........................ {Root.Instance.Scene.Id}");
                
// 根据当初命令行的参数来的：是真根据命令行来的【先前某监控服务器Watcher、对宕机重启的服务端，使用的好像是命令行？去把逻辑找出来】？还是根据配置文件 Excel 开各小服来的？
                switch (Options.Instance.AppType) { 
                case AppType.ExcelExporter: {
                    Options.Instance.Console = 1;
                    ExcelExporter.Export();
                    return 0;
                }
                case AppType.Proto2CS: {
                    Options.Instance.Console = 1;
                    Proto2CS.Export();
                    return 0;
                }
                }
            }
            catch (Exception e) {
                Log.Console(e.ToString());
            }
            return 1;
        }
    }
}