using System;
using System.Collections.Generic;
using System.Net;
namespace ET.Server {
// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
	// 【TODO】：要看懂RouterManager 场景里，这个组件的作用、功用
	// 【TODO】：网络、路由相关的 2 个模块，是亲爱的表哥的活宝妹现在最大的难点。多看几个早上，是会、能够把它们都看懂读懂的！
	[FriendOf(typeof(HttpComponent))] //indent 出错原因是：笨 emacs csharp-mode 不识别【标号！
    public static class HttpComponentSystem {

        public class HttpComponentAwakeSystem : AwakeSystem<HttpComponent, string> {
            protected override void Awake(HttpComponent self, string address) {
                try {
                    self.Load();

                    self.Listener = new HttpListener();
                    foreach (string s in address.Split(';')) {
                        if (s.Trim() == "") {
                            continue;
                        }
                        self.Listener.Prefixes.Add(s);
                    }
                    self.Listener.Start(); // 【TODO】：这些细节，就不太懂
                    self.Accept().Coroutine();
                }
                catch (HttpListenerException e) {
                    throw new Exception($"请先在cmd中运行: netsh http add urlacl url=http:// *:你的address中的端口/ user=Everyone, address: {address}", e);
                }
            }
        }
        [ObjectSystem]
        public class HttpComponentLoadSystem: LoadSystem<HttpComponent> {
            protected override void Load(HttpComponent self) {
                self.Load();
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
            self.dispatcher = new Dictionary<string, IHttpHandler>(); // 虽然框架里，只有一个处理器，仍然用字典管理
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (HttpHandlerAttribute));
            SceneType sceneType = (self.Parent as IScene).SceneType; // 当前组件、所属的场景
            foreach (Type type in types) {
                object[] attrs = type.GetCustomAttributes(typeof(HttpHandlerAttribute), false);
                if (attrs.Length == 0) {
                    continue;
                }
                HttpHandlerAttribute httpHandlerAttribute = (HttpHandlerAttribute)attrs[0];
                if (httpHandlerAttribute.SceneType != sceneType) { // 不是这个场景的处理器
                    continue;
                }
                object obj = Activator.CreateInstance(type);
                IHttpHandler ihttpHandler = obj as IHttpHandler;
                if (ihttpHandler == null) {
                    throw new Exception($"HttpHandler handler not inherit IHttpHandler class: {obj.GetType().FullName}");
                }
                self.dispatcher.Add(httpHandlerAttribute.Path, ihttpHandler); // 把路径，处理器加入字典
            }
        }
        public static async ETTask Accept(this HttpComponent self) {
            long instanceId = self.InstanceId;
            while (self.InstanceId == instanceId) { // 什么意思呢？只要自己当前组件还没有回收？
                try {
                    HttpListenerContext context = await self.Listener.GetContextAsync();
                    self.Handle(context).Coroutine();
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
                IHttpHandler handler;
                if (self.dispatcher.TryGetValue(context.Request.Url.AbsolutePath, out handler)) {
                    await handler.Handle(self.DomainScene(), context);
                }
            }
            catch (Exception e) {
                Log.Error(e);
            }
            context.Request.InputStream.Dispose();
            context.Response.OutputStream.Dispose();
        }
    }
}
// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
