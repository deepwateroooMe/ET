using System;
using System.Collections.Generic;
using System.Net;
namespace ET.Server {
    [FriendOf(typeof(HttpComponent))]
    public static class HttpComponentSystem {
        public class HttpComponentAwakeSystem : AwakeSystem<HttpComponent, string> {
            protected override void Awake(HttpComponent self, string address) {
                try {
                    self.Load(); // <<<<<<<<<<<<<<<<<<<< 
                    self.Listener = new HttpListener(); // 它是服务端管理组件，理解为【专职来监听客户端各小服的】（不知道它是否，开启了一个任务线程？）感觉应该是。看下面的
                    foreach (string s in address.Split(';')) {
                        if (s.Trim() == "") 
                            continue;
                        self.Listener.Prefixes.Add(s);
                    }
                    self.Listener.Start();
                    self.Accept().Coroutine(); // <<<<<<<<<<<<<<<<<<<< 异步接收【客户端】各小服上报。。那么至少，这个异步接收的，是个任务线程？！！
                }
                catch (HttpListenerException e) {
                    throw new Exception($"请先在cmd中运行: netsh http add urlacl url=http:// *:你的address中的端口/ user=Everyone, address: {address}", e);
                }
            }
        }
        [ObjectSystem]
        public class HttpComponentLoadSystem: LoadSystem<HttpComponent> {
            protected override void Load(HttpComponent self) {
                self.Load(); // <<<<<<<<<<<<<<<<<<<< 
            }
        }
        [ObjectSystem]
        public class HttpComponentDestroySystem: DestroySystem<HttpComponent> {
            protected override void Destroy(HttpComponent self) {
                self.Listener.Stop();
                self.Listener.Close();
            }
        }
        public static void Load(this HttpComponent self) {
            self.dispatcher = new Dictionary<string, IHttpHandler>(); // 初始化管理字典 
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (HttpHandlerAttribute)); // 实则，全局只有一个 HttpGetRouterHandler
            SceneType sceneType = self.GetParent<Scene>().SceneType; // 【RouterManager】场景，才会添加这个组件。框架里只有这个场景，添加过这个组件
            foreach (Type type in types) {
                object[] attrs = type.GetCustomAttributes(typeof(HttpHandlerAttribute), false);
                if (attrs.Length == 0) 
                    continue;
                HttpHandlerAttribute httpHandlerAttribute = (HttpHandlerAttribute)attrs[0];
                if (httpHandlerAttribute.SceneType != sceneType) 
                    continue;
                object obj = Activator.CreateInstance(type); // 创建一个处理器实例
                IHttpHandler ihttpHandler = obj as IHttpHandler;
                if (ihttpHandler == null) 
                    throw new Exception($"HttpHandler handler not inherit IHttpHandler class: {obj.GetType().FullName}");
                self.dispatcher.Add(httpHandlerAttribute.Path, ihttpHandler); // "/get_router": 把【路径、处理器】加入管理系统
            }
        }
        public static async ETTask Accept(this HttpComponent self) { // 还是本类上面调用的
            long instanceId = self.InstanceId;
            while (self.InstanceId == instanceId) { // 只要当前这个【路由器管理器场景的 http组件】没有发生变化，就一直进行。。。
                try {
                    HttpListenerContext context = await self.Listener.GetContextAsync(); // 刚才，有个帮助类，不是把什么结果写进上下文了，没有不必发回消息吗？这里【异步读到】
                    self.Handle(context).Coroutine(); // <<<<<<<<<<<<<<<<<<<< 调用下面的方法：并【异步处理】上下文中返回的消息 
                }
                catch (ObjectDisposedException) {
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        public static async ETTask Handle(this HttpComponent self, HttpListenerContext context) {
            try {
                IHttpHandler handler; // 全框架：现在也只有一个实现类 HttpGetRouterHandler.cs(属于RouterManager 场景 ). 那么，应该就是调用它所定义的处理方法。 
                if (self.dispatcher.TryGetValue(context.Request.Url.AbsolutePath, out handler)) 
                    await handler.Handle(self.Domain as Scene, context); // 调用注册过、生成的【HttpHandler】标签实例，的处理方法来回调。【异步方法 】
            }
            catch (Exception e) {
                Log.Error(e);
            }
            context.Request.InputStream.Dispose(); // 上面【异步方法】处理完了，就可以回收了
            context.Response.OutputStream.Dispose();
        }
    }
}