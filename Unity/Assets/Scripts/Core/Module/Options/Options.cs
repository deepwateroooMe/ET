using CommandLine;
using System;
using System.Collections.Generic;
namespace ET {
    public enum AppType {
        Server,
        Watcher, // 每台物理机一个守护进程，用来启动该物理机上的所有进程
        GameTool,
        ExcelExporter,
        Proto2CS,
        BenchmarkClient,
        BenchmarkServer,
        // 下面是我重新添加的：可是添加这些有什么用呢？
        Gate, 
        Realm, 
        Map, 
        Match
    }
    public class Options: Singleton<Options> { // 这个【单例类】，是使用命令行命令启动【服务端】来根据命令行参数解析出参数值的全局单例类。它属于谁？场景？活宝妹属于亲爱的表哥！！！
        [Option("AppType", Required = false, Default = AppType.Server, HelpText = "AppType enum")]
        public AppType AppType { get; set; }
        // StartConfig 初始配置的路径、命令行参数：参数不是必需。缺省为 StartConfig/Localhost 嵌套文件夹路径，字符串
        [Option("StartConfig", Required = false, Default = "StartConfig/Localhost")]
        public string StartConfig { get; set; }
        [Option("Process", Required = false, Default = 1)]
        public int Process { get; set; }
        
        [Option("Develop", Required = false, Default = 0, HelpText = "develop mode, 0正式 1开发 2压测")]
        public int Develop { get; set; }
        [Option("LogLevel", Required = false, Default = 2)]
        public int LogLevel { get; set; }
        [Option("Console", Required = false, Default = 0)]
        public int Console { get; set; }
        // 进程启动是否创建该进程的scenes
        [Option("CreateScenes", Required = false, Default = 1)]
        public int CreateScenes { get; set; }
    }
}