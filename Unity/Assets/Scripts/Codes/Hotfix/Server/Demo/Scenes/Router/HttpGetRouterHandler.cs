using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
namespace ET.Server {
    // 【路由器管理器场景】：热更域里，帮助【动态路由器系统】扫描周围邻居的帮助方法类
    // 调用的地方在 RouterAddressComponentSystem.cs 里，会想从这里，从管理处？拿网络里的【路由表】
    // 【异步方法】：物理机以IP 地址相区分，同一物理机上的不同进程，如果端口不复用，以端口相区分。
        // 这里去想：异步方法时，不同物理机，是如何一个一个把各自路由整合起来的？这里想得不对
    // 【服务端】启动时，先前分【四大主要管理类】＋其它小杂琐，是写在单例管理类里的。这里直接去读，先前写过的信息。
    [HttpHandler(SceneType.RouterManager, "/get_router")] // 【路由器】专用的，管理场景 
    public class HttpGetRouterHandler : IHttpHandler {

        // 【框架原始方法定义】如下：
        // public async ETTask Handle(Entity domain, HttpListenerContext context) // 这里，搞不清楚 domain 是什么意思，先传个场景进来
        // 要本管去处理，本管就：从最初的各配置管理单例里【这里问题变成是：实时过程中，各小服上报过程中，这些配置，是否，能够实时更新？应该是可以的】，去读，读到了，我就写回去！！爱表哥，爱生活！！！
        // 【这里问题变成是：实时过程中，各小服上报过程中，这些配置，是否，能够实时更新？应该是可以的】：因为任何过程中小服上报的过程，仍然是跨进程消息上报？跨进程消化反序列化结束，就会写入各配置管理单例里，是实时更新的【爱表哥，爱生活！！！】
        public async ETTask Handle(Scene scene, HttpListenerContext context) {
            HttpGetRouterResponse response = new HttpGetRouterResponse(); // HttpGetRouterResponse 类：是框架自定义的，用来管理路由表的三条链表
            // response.Realms = new List<string>();
            // response.Matchs = new List<string>();// 匹配服链表  // <<<<<<<<<<<<<<<<<<<< 
            response.Routers = new List<string>();
            // 是去StartSceneConfigCategory 这里拿的【它不是全局单例，它是ConfigSigleton?】：
            // 因为它可以 proto 消息里、进程间传递，传递的逻辑与过程，应该是在ProtoObject 跨进程消息【反序列化】结束之后，添加到ConfigSigleton 的
            // 那么，【服务端】的启动过程（或说动态路由扫描过程）仍是【自底向上】各小服，跨进程消息，上报的过程
            // 现在的理解：就变成为，跨进程，ProtoObject Partial 类定义，合并跨进程消化的过程？是？？？
            response.Realm = StartSceneConfigCategory.Instance.Realm.InnerIPOutPort.ToString();
            // foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Realms) {
            //     response.Realms.Add(startSceneConfig.InnerIPOutPort.ToString()); // 异步方法，同物理机同核同进程，多场景，添加进链表，可以直接加的？同进程自动多线程安全管理？
            // }
            response.Match = StartSceneConfigCategory.Instance.Match.InnerIPOutPort.ToString(); // 这个【匹配服】：全局唯一
            // foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Match) {
            //     response.Matchs.Add(startSceneConfig.InnerIPOutPort.ToString());
            // }
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Routers) {
                response.Routers.Add($"{startSceneConfig.StartProcessConfig.OuterIP}:{startSceneConfig.OuterPort}");
            }
// 把这个返回消息写好了，下文呢？需要发吗，还是http 的底层有相关逻辑，自动处理呢？感觉像异步返回消息写好了，当时找不到怎么发回去的一样
            HttpHelper.Response(context, response); // <<<<<<<<<<<<<<<<<<<< 把写好的消息，跨进程返回去
            await ETTask.CompletedTask; // 骗编译器说：我是异步方法
        }
    }
}