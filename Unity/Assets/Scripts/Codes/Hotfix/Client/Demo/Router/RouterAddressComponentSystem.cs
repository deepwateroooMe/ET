using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
namespace ET.Client {
	
    [FriendOf(typeof(RouterAddressComponent))]
    public static class RouterAddressComponentSystem {
		
        public class RouterAddressComponentAwakeSystem: AwakeSystem<RouterAddressComponent, string, int> {
			// Awake() 时，传入 Manager 的相关信息
            protected override void Awake(RouterAddressComponent self, string address, int port) {
                self.RouterManagerHost = address;
                self.RouterManagerPort = port;
            }
        }
        public static async ETTask Init(this RouterAddressComponent self) {
            self.RouterManagerIPAddress = NetworkHelper.GetHostAddress(self.RouterManagerHost);
            await self.GetAllRouter();
        }

        private static async ETTask GetAllRouter(this RouterAddressComponent self) { // 扫了两次；间隔 10 分钟
            string url = $"http:// {self.RouterManagerHost}:{self.RouterManagerPort}/get_router?v={RandomGenerator.RandUInt32()}";
            Log.Debug($"start get router info: {url}");
            string routerInfo = await HttpClientHelper.Get(url); // 是C# 。NET 的一个公用API, 实在不懂底层原理就先放一下
            Log.Debug($"recv router info: {routerInfo}");
            HttpGetRouterResponse httpGetRouterResponse = JsonHelper.FromJson<HttpGetRouterResponse>(routerInfo); // Parse 成这个类
            self.Info = httpGetRouterResponse;
            Log.Debug($"start get router info finish: {JsonHelper.ToJson(httpGetRouterResponse)}");
            // 打乱顺序
            RandomGenerator.BreakRank(self.Info.Routers);
            self.WaitTenMinGetAllRouter().Coroutine(); // 等 5 分钟，又扫了一次
        }
        // 等10分钟再获取一次
        public static async ETTask WaitTenMinGetAllRouter(this RouterAddressComponent self) {
            await TimerComponent.Instance.WaitAsync(5 * 60 * 1000); // 等5 分钟
            if (self.IsDisposed) {
                return;
            }
			// 【TODO】：这里等 5 分钟，再扫一次的过程，总觉得少掉了【服务端】自底向上、各小集中信息的过程？需要去确认确认
            await self.GetAllRouter(); // 再扫：可能也需要 5 分钟左右。就是与，一个大型【动态路由】网络里，路由器间的收敛速度相关
        }
        public static IPEndPoint GetAddress(this RouterAddressComponent self) {
            if (self.Info.Routers.Count == 0) 
                return null;
			// 这里，【客户端】与【Realms 服】连接，所使用的路由器的地址，仍然是【随机分配】的
            string address = self.Info.Routers[self.RouterIndex++ % self.Info.Routers.Count];
            string[] ss = address.Split(':');
            IPAddress ipAddress = IPAddress.Parse(ss[0]);
            if (self.RouterManagerIPAddress.AddressFamily == AddressFamily.InterNetworkV6) { 
                ipAddress = ipAddress.MapToIPv6();
            }
            return new IPEndPoint(ipAddress, int.Parse(ss[1]));
        }
        public static IPEndPoint GetRealmAddress(this RouterAddressComponent self, string account) {
            int v = account.Mode(self.Info.Realms.Count); // 随机分配了一个 Realms 给【客户端】用
            string address = self.Info.Realms[v];
            string[] ss = address.Split(':');
            IPAddress ipAddress = IPAddress.Parse(ss[0]);
            // if (self.IPAddress.AddressFamily == AddressFamily.InterNetworkV6)
            // { 
            //    ipAddress = ipAddress.MapToIPv6();
            // }
            return new IPEndPoint(ipAddress, int.Parse(ss[1]));
        }
    }
} // 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】