using System.Collections.Generic;
using System.Net;
namespace ET.Server {
	// 亲爱的表哥的活宝妹，就是自己想不明白，这几个模块，是在干什么。。。【TODO】：
	
    // http请求分发器【源】：
    [ComponentOf(typeof(Scene))]
    public class HttpComponent: Entity, IAwake<string>, IDestroy, ILoad {
        public HttpListener Listener;
        public Dictionary<string, IHttpHandler> dispatcher;
    }
}