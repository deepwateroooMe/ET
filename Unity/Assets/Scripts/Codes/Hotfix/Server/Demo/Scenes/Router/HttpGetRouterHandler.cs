using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
namespace ET.Server {
    // 【路由器管理器场景】：热更域里，帮助【动态路由器系统】扫描周围邻居的帮助方法类
    [HttpHandler(SceneType.RouterManager, "/get_router")]
    public class HttpGetRouterHandler : IHttpHandler {
        // 【框架原始方法定义】如下
        // public async ETTask Handle(Entity domain, HttpListenerContext context)
        public async ETTask Handle(Scene scene, HttpListenerContext context) {
            HttpGetRouterResponse response = new HttpGetRouterResponse();
            response.Realms = new List<string>();
            response.Matchs = new List<string>();// 匹配服链表  // <<<<<<<<<<<<<<<<<<<< 
            response.Routers = new List<string>();
            // 是去StartSceneConfigCategory 这里拿的：因为它可以 proto 消息里、进程间传递，这里还不是狠懂，这个东西存放在哪里？
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Realms) {
                response.Realms.Add(startSceneConfig.InnerIPOutPort.ToString());
            }
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Matchs) {
                response.Matchs.Add(startSceneConfig.InnerIPOutPort.ToString());
            }
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Routers) {
                response.Routers.Add($"{startSceneConfig.StartProcessConfig.OuterIP}:{startSceneConfig.OuterPort}");
            }
// 把这个返回消息写好了，下文呢？需要发吗，还是http 的底层有相关逻辑，自动处理呢？感觉像异步返回消息写好了，当时找不到怎么发回去的一样
            HttpHelper.Response(context, response); // <<<<<<<<<<<<<<<<<<<< 把写好的消息，跨进程返回去
            await ETTask.CompletedTask;
        }
    }
}
