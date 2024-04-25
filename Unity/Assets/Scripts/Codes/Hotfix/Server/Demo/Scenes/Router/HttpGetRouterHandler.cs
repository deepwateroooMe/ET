using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ET.Server {
	
	// 框架里，只有这一个【实现类】，它是从【服务端】启动时，根据 Excel 配置文件加载服务端各进程各场景后、汇总的配置，来返回给调用方
    [HttpHandler(SceneType.RouterManager, "/get_router")]
    public class HttpGetRouterHandler : IHttpHandler {

        public async ETTask Handle(Scene scene, HttpListenerContext context) {
            HttpGetRouterResponse response = new HttpGetRouterResponse();
            response.Realms = new List<string>();
            response.Routers = new List<string>();
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Realms) {
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
