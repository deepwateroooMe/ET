using System;
using System.Threading;
using CommandLine;
using UnityEngine;

namespace ET {

    public class Init: MonoBehaviour {

        private void Start() {
            DontDestroyOnLoad(gameObject);
            
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                Log.Error(e.ExceptionObject.ToString());
            };
                
            Game.AddSingleton<MainThreadSynchronizationContext>(); // 线程上下文的无缝切换，可以高枕无忧不用管了
            // 命令行参数
            string[] args = "".Split(" ");
            Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                .WithParsed(Game.AddSingleton);

// 注意，每个被Add的组件，都会执行其Awake（前提是他有类似的方法），这也是ETBook中的内容，不懂的同学回去补课哦
            Game.AddSingleton<TimeInfo>();
            Game.AddSingleton<Logger>().ILog = new UnityLogger();
            Game.AddSingleton<ObjectPool>();
            Game.AddSingleton<IdGenerater>();
            Game.AddSingleton<EventSystem>();
            Game.AddSingleton<TimerComponent>();
            Game.AddSingleton<CoroutineLockComponent>();
            
            ETTask.ExceptionHandler += Log.Error;
            Game.AddSingleton<CodeLoader>().Start();
        }
        
// 框架中关注过的，几个统一管理的生命周期回调函数的一致系统化管理调用. 
        private void Update() {
            Game.Update(); // <<<<<<<<<< 
        }
        private void LateUpdate() {
            Game.LateUpdate(); // <<<<<<<<<< 
            Game.FrameFinishUpdate(); // <<<<<<<<<< 
        }
        private void OnApplicationQuit() {
            Game.Close(); // <<<<<<<<<< 
        }
    }
}