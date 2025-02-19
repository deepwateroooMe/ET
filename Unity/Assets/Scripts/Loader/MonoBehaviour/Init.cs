﻿using System;
using System.Threading;
using CommandLine;
using UnityEngine;
namespace ET {
    // 【客户端】的起始程序 
    public class Init: MonoBehaviour {
        private void Start() {
            DontDestroyOnLoad(gameObject);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                Log.Error(e.ExceptionObject.ToString());
            };
            Game.AddSingleton<MainThreadSynchronizationContext>();
            string[] args = "".Split(" "); // 命令行参数
            Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
                .WithParsed(Game.AddSingleton);
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
        private void Update() {
            Game.Update();
        }
        private void LateUpdate() {
            Game.LateUpdate();
            Game.FrameFinishUpdate();
        }
        private void OnApplicationQuit() {
            Game.Close();
        }
    }
}