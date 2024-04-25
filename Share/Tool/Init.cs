using System;
using System.Collections.Generic;
using System.Threading;
using CommandLine;
namespace ET.Server {
    internal static class Init {
        private static int Main(string[] args) {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                Log.Error(e.ExceptionObject.ToString());
            };
            try {
                // 异步方法全部会回掉到主线程
                Game.AddSingleton<MainThreadSynchronizationContext>();
                
                // 命令行参数【源】：【TODO】：这里也还是没太看懂
                Parser.Default.ParseArguments<Options>(args)
                    .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                    .WithParsed(Game.AddSingleton);
                
                Game.AddSingleton<TimeInfo>();
                Game.AddSingleton<Logger>().ILog = new NLogger(Options.Instance.AppType.ToString(), Options.Instance.Process, "../Config/NLog/NLog.config");
                Game.AddSingleton<ObjectPool>();
                Game.AddSingleton<IdGenerater>();
                
                ETTask.ExceptionHandler += Log.Error; // ETTask 异常处理逻辑：打印日志
                
                Game.AddSingleton<EventSystem>();
                Dictionary<string, Type> types = AssemblyHelper.GetAssemblyTypes(typeof (Game).Assembly);
                EventSystem.Instance.Add(types);
                
                Game.AddSingleton<Root>();
                MongoHelper.Init();
                ProtobufHelper.Init();
                
                Log.Info($"server start........................ {Root.Instance.Scene.Id}");
				// 下面，这些，【帮助工具】：各自会有个、几乎专用进程
                switch (Options.Instance.AppType) { // AppType 进程级别的
// 今天下午第1 件事：重点把这个帮助项目、工具类的、前世今生。。都给努力看明白、读明白。。【TODO】：现在！
				case AppType.ExcelExporter: { // 【服务端】的2 个帮助工具类项目： Proto2CS 和 ExcelExporter
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