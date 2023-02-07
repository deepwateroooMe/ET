using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ET.Server {

    [HttpHandler(SceneType.RouterManager, "/get_router")]
    public class HttpGetRouterHandler : IHttpHandler {

        public async ETTask Handle(Entity domain, HttpListenerContext context) {
// 氢现存在的,注册过的配置过的,所有的Realms, Routers给返回回去,深复制,返回一个新实例版本
            HttpGetRouterResponse response = new HttpGetRouterResponse();
            response.Realms = new List<string>();
            response.Routers = new List<string>();
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Realms) { // <<<<<<<<<< 这里就是初始化的时候配置的单例,那么所有的Realm都这里有纪录的
                response.Realms.Add(startSceneConfig.InnerIPOutPort.ToString());
            }
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Routers) {
                response.Routers.Add($"{startSceneConfig.StartProcessConfig.OuterIP}:{startSceneConfig.OuterPort}");
            }
            HttpHelper.Response(context, response);
            await ETTask.CompletedTask;
        }
    }
}
