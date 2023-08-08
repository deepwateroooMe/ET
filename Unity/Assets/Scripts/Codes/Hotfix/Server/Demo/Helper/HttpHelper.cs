using System.Net;
using System.Text;
namespace ET.Server {
    public static class HttpHelper {
        // 这里已前可能没读懂：当把 context 中的 Response 写好，写进OutputStream 输出流里去，底层方法大概会有自动读取什么的，再底层的可以不用管了
        // 感觉框架里这部分，可能还是相对粗糙，比较简单？
        public static void Response(HttpListenerContext context, object response) {
            byte[] bytes = JsonHelper.ToJson(response).ToUtf8();
            context.Response.StatusCode = 200;
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        }
    }
}