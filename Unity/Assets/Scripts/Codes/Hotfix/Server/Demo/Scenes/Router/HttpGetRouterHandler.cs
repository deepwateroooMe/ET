using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ET.Server {
	
	// 框架里，只有这一个【实现类】，它是从【服务端】启动时，根据 Excel 配置文件加载服务端各进程各场景后、汇总的配置，来返回给调用方
	// 回调的触发，是当任何一个【普通路由器】连入网络，想要从Manager 处拿，现在【动态路由】网络里的所有路由器时，就会触发这里的执行
    [HttpHandler(SceneType.RouterManager, "/get_router")]
    public class HttpGetRouterHandler : IHttpHandler {

        public async ETTask Handle(Scene scene, HttpListenerContext context) {
            HttpGetRouterResponse response = new HttpGetRouterResponse();
            response.Realms = new List<string>();
            response.Routers = new List<string>();
			// 把【服务端】启动过程中，扫到过的、保存过的 Realms Routers 相关信息，直接返回 
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Realms) {
                response.Realms.Add(startSceneConfig.InnerIPOutPort.ToString());
            }
            foreach (StartSceneConfig startSceneConfig in StartSceneConfigCategory.Instance.Routers) {
                response.Routers.Add($"{startSceneConfig.StartProcessConfig.OuterIP}:{startSceneConfig.OuterPort}");
            }

            HttpHelper.Response(context, response); // 静态帮助类：帮助把HttpResponse 的异步结果写好
            await ETTask.CompletedTask;
        }
    }
}
