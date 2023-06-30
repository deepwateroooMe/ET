using System.Collections.Generic;
using System.Net;
namespace ET.Server {
    // http请求分发器
    [ComponentOf(typeof(Scene))]
    public class HttpComponent: Entity, IAwake<string>, IDestroy, ILoad {
        public HttpListener Listener;
        public Dictionary<string, IHttpHandler> dispatcher; // 管理各种回调，吗？
    }
}