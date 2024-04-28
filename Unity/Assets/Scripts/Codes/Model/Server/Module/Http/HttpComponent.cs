using System.Collections.Generic;
using System.Net;
namespace ET.Server {

	// RouterManager 的组件：用来处理，来自其它路由场景的、请求的
	
    // http请求分发器【源】：
    [ComponentOf(typeof(Scene))]
    public class HttpComponent: Entity, IAwake<string>, IDestroy, ILoad {
        public HttpListener Listener;
        public Dictionary<string, IHttpHandler> dispatcher;
    }
}