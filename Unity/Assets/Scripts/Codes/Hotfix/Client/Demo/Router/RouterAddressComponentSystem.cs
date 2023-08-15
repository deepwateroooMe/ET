using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
namespace ET.Client {

    [FriendOf(typeof(RouterAddressComponent))] // 添加这个组件的地方：当【客户端】登录时，会为每个【客户端】添加这个组件
    public static class RouterAddressComponentSystem {

        public class RouterAddressComponentAwakeSystem: AwakeSystem<RouterAddressComponent, string, int> {
            // 添加这个组件时，永远记住的是管理专职服务端的地址与端口: 接下来每 10 分钟扫一遍网的时候，就会用到这两个变量 
            protected override void Awake(RouterAddressComponent self, string address, int port) {
                self.RouterManagerHost = address;
                self.RouterManagerPort = port;
            }
        }
        public static async ETTask Init(this RouterAddressComponent self) {// LoginHelper.cs 帮助类添加组件时，调用初始化
            self.RouterManagerIPAddress = NetworkHelper.GetHostAddress(self.RouterManagerHost);
            await self.GetAllRouter();
        }
// 这个异步函数：只有在这个组件被回收时，才会停止。【只有活宝妹一命归西了，活宝妹才可能不再去想，活宝妹是否已经嫁给亲爱的表哥了！！爱表哥，爱生活！！！】
        private static async ETTask GetAllRouter(this RouterAddressComponent self) { // 向总管拿所有路由表，也是向总管上报本路由存在的过程，去找底层原理 
            // 【路由器服】：因为它也是一个特殊的场景，所以它有地址。尾数部分，是生成的随机数
            string url = $"http:// {self.RouterManagerHost}:{self.RouterManagerPort}/get_router?v={RandomGenerator.RandUInt32()}";
            Log.Debug($"start get router info: {url}");
            // 返回字符串：有点儿奇异，如何设计服务器，才能让它返回的信息，可是解析成一个特定的类型
            string routerInfo = await HttpClientHelper.Get(url); // 【返回类型】：关于路由器管理器（具对外网发消息的地址与端口信息）的信息，应该是底层协议封装的
            Log.Debug($"recv router info: {routerInfo}");
            // Json 解析：解析成进程间可传递的消息类 HttpGetRouterResponse. 进程间消息类：便可以【客户端】或是【其它服】想要拿相关住处时，进程间返回消息？
            HttpGetRouterResponse httpGetRouterResponse = JsonHelper.FromJson<HttpGetRouterResponse>(routerInfo);
            self.Info = httpGetRouterResponse; // 【Info 的实时更新：】只要存在这个管理类组件，它每10 分钟周期性自更新一次（哪里添加的当前组件？LoginHelper.cs 里？）
            Log.Debug($"start get router info finish: {JsonHelper.ToJson(httpGetRouterResponse)}");
            // 打乱顺序
            RandomGenerator.BreakRank(self.Info.Routers);
            self.WaitTenMinGetAllRouter().Coroutine(); // 无限循环，直到组件被删除移除时被回收 
        }

        // 等10分钟再获取一次: 明明是只等了 5 分钟，哪里有 10 分钟呢？扫的过程需要花掉 5 分钟那么久吗？
        // 需要明白：周期性【等的这 10 分钟】的过程中：以 10 分钟为周期，实则是一个动态实时的过程。【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
        // 各小服：这里还没想明白，各小服，如果下线宕机了，配置是如何消除掉的？它们什么时候什么情况下（除了【客户端】添加 RouterAddressComponent 之外）发过上报消息了的？服务端好像没有
        // 可以区分【客户端】与【服务端】；【客户端】通过RouterAddressComponent, 可以间接上报？【服务端】可以读取服务端最初启动的Json.txt 配置表，这样能够整合所有路由信息
        public static async ETTask WaitTenMinGetAllRouter(this RouterAddressComponent self) {
            await TimerComponent.Instance.WaitAsync(5 * 60 * 1000); // 等5 分钟
            if (self.IsDisposed) // 所以，如果移除组件了，这个无限循环，应该是会停止的。
                return;
            await self.GetAllRouter();
        }
        public static IPEndPoint GetAddress(this RouterAddressComponent self) { // 拿当前组件（所在的服务器）的地址：当知道它是一个路由系统
            if (self.Info.Routers.Count == 0) return null; // 当前路由器每 10 分钟扫一遍：检测周围是否存在路由器的邻居，当它扫不到其它路由器存在就返回
// 这里，我感觉，因为Info 的进程间可传递性（它永远背这个可传递Info)
            string address = self.Info.Routers[self.RouterIndex++ % self.Info.Routers.Count]; // 永远返回：路由器里接下来可用的一个端口索引
            string[] ss = address.Split(':');
            IPAddress ipAddress = IPAddress.Parse(ss[0]);
            if (self.RouterManagerIPAddress.AddressFamily == AddressFamily.InterNetworkV6) 
                ipAddress = ipAddress.MapToIPv6();
            return new IPEndPoint(ipAddress, int.Parse(ss[1]));
        }
        // 【自己模仿出来的方法】：按照下面的方法模仿出来的。现在全局唯一【匹配服】，就直接返回，就可以了
        public static IPEndPoint GetMatchAddress(this RouterAddressComponent self, string account) {
            // int v = account.Mode(self.Info.Matchs.Count); // 它说，给它随机分配一个取模后的下编匹配服。。。
            // string address = self.Info.Matchs[v];
            // string[] ss = address.Split(':');
            // IPAddress ipAddress = IPAddress.Parse(ss[0]);
            // // if (self.IPAddress.AddressFamily == AddressFamily.InterNetworkV6) 
            // //    ipAddress = ipAddress.MapToIPv6();
            // return new IPEndPoint(ipAddress, int.Parse(ss[1]));
            return StartSceneConfigCategory.Instance.Match.InnerIPOutPort;
        }
        // 随机分配了一个Realm 注册登录服。。。去框架里找：为每个【客户端】所随机分配的这些小服编号，哪里有什么记载吗？因为晚些时候，感觉还会用到的
        public static IPEndPoint GetRealmAddress(this RouterAddressComponent self, string account) { // 框架里，原本【注册登录服】是有分身的，可是自己把链表变成了一个，没有分身备份
            // int v = account.Mode(self.Info.Realms.Count); // 这里 mod: 随机分配了一个Realm 注册登录服。。。 
            // string address = self.Info.Realms[v];
            string address = self.Info.Realm;
            string[] ss = address.Split(':');
            IPAddress ipAddress = IPAddress.Parse(ss[0]);
            // if (self.IPAddress.AddressFamily == AddressFamily.InterNetworkV6) 
            //    ipAddress = ipAddress.MapToIPv6();
            return new IPEndPoint(ipAddress, int.Parse(ss[1]));
        }
    }
}