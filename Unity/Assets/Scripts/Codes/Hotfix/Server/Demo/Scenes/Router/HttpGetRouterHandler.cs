using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
namespace ET.Server
{
    //【专用路由器管理场景】：是客户端会发消息过来要拿地址吗？它就把请求消息返回去。我应该去把发请求消息，要拿地址的地方找出来 
    [HttpHandler(SceneType.RouterManager, "/get_router")]
    public class HttpGetRouterHandler : IHttpHandler
    {
        // 它说：申明过的接口是这个，但是这个接口没有具体实现. 反正它第一个参数没用，就直接换一下
        public async ETTask Handle(Scene scene, HttpListenerContext context)// => throw new NotImplementedException();
        // public async ETTask Handle(Entity domain, HttpListenerContext context)
        {
            HttpGetRouterResponse response = new HttpGetRouterResponse();
            response.Realms = new List<string>();
            response.Matchs = new List<string>();// 匹配服链表  // <<<<<<<<<<<<<<<<<<<< 
            response.Routers = new List<string>();
            // 是去StartSceneConfigCategory 这里拿的：因为它可以 proto 消息里、进程间传递，这里还不是狠懂，这个东西存放在哪里？
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Realms)
            {
                response.Realms.Add(startSceneConfig.InnerIPOutPort.ToString());
            }
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Matchs)
            {
                response.Matchs.Add(startSceneConfig.InnerIPOutPort.ToString());
            }
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Routers)
            {
                response.Routers.Add($"{startSceneConfig.StartProcessConfig.OuterIP}:{startSceneConfig.OuterPort}");
            }
            HttpHelper.Response(context, response);
            await ETTask.CompletedTask;
        }
    }
}
