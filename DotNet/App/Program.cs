using System;
using System.Threading;
namespace ET {
    public static class Program {
        // 【都看明白了】：
        public static void Main() { // DOTNET 应用程序的总入口：
            // 这里大家看着可能比较困惑，为什么要绕一大圈呢，之前这里直接调用Model层，现在却要在CoderLoader中获取Model的程序集找到Entry入口再调用
            // 原因是，之前DotNet.App直接依赖Model，但是在客户端，之前的Mono却不依赖Model。这导致前端跟后端程序集依赖不太一样
            // 所以这次加了个Loader的程序集，客户端的Mono程序集也改成Loader，这样前后端Model都引用Loader，Loader通过反射去调用Model的Entry。
            // 这样前后端的程序集依赖就保持了一致。这里调用了Entry.Init()是为了防止dotnet裁剪Model的程序集，毕竟如果App没有调用model，那么dotnet认为
            // model并没有用到，就不会加载，结果会导致CodeLoader反射调用model失败。
            // 客户端服务端不热更不共享的组件可以写到Loader中，比如表现层需要一个组件不需要热更，可以写在Loader中，这样性能更高。如果客户端跟服务端共享的并且不需要热更的
            // 的组件可以写在Core中
// 【防程序域不被加载的招儿】：调一次Model 域里的空方法。是为了防止dotnet裁剪Model的程序集，毕竟如果App没有调用model，那么dotnet认为model并没有用到，就不会加载
            Entry.Init(); // <<<<<<<<<<<<<<<<<<<< 防 dotnet 程序域不加载Model 程序载，所有手动为 dotnet 添加一个对Model 的引用，哪怕是调用空方法
            Init.Start(); // 【服务端】静态起始类：类里通过 CodeLoader 中转
            // Unity Framework里有各种回调；ET 框架的各种自定义回调，也基本模拟了Unity; 它定义了双端每桢必做的事情等几个回调；【服务端】的这些回调与基于Unity 出来的【客户端】各回调，像插口一一对应起来。下面几个帮助类都可以想成是链路，以及实现双端一致性的链路
            while (true) {
                Thread.Sleep(1);
                try { // 下面的：都是链接Unity Update()|LateUpdate()|FrameFinishUpdate() 等回调，链接到框架里相应自定义回调方法，的链路
                    Init.Update();
                    Init.LateUpdate();
                    Init.FrameFinishUpdate();
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
    }
}