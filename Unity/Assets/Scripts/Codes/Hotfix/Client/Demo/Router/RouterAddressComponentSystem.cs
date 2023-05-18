using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
namespace ET.Client {
    [FriendOf(typeof(RouterAddressComponent))]
    public static class RouterAddressComponentSystem {
        public class RouterAddressComponentAwakeSystem: AwakeSystem<RouterAddressComponent, string, int> {
            protected override void Awake(RouterAddressComponent self, string address, int port) {
                self.RouterManagerHost = address;
                self.RouterManagerPort = port;
            }
        }
        public static async ETTask Init(this RouterAddressComponent self) {
            self.RouterManagerIPAddress = NetworkHelper.GetHostAddress(self.RouterManagerHost);
            await self.GetAllRouter();
        }
        private static async ETTask GetAllRouter(this RouterAddressComponent self) { // 这个异步函数：生生世世，无限轮回
            // 【路由器服】：吗，因为它也是一个特殊的场景，所以它有地址
            string url = $"http:// {self.RouterManagerHost}:{self.RouterManagerPort}/get_router?v={RandomGenerator.RandUInt32()}";
            Log.Debug($"start get router info: {url}");
            // 返回字符串：有点儿奇异，如何设计服务器，才能让它返回的信息，可是解析成一个特定的类型
            string routerInfo = await HttpClientHelper.Get(url);
            Log.Debug($"recv router info: {routerInfo}");
            // Json 解析：解析成 proto 可传递的消息类 HttpGetRouterResponse
            HttpGetRouterResponse httpGetRouterResponse = JsonHelper.FromJson<HttpGetRouterResponse>(routerInfo);
            self.Info = httpGetRouterResponse;
            Log.Debug($"start get router info finish: {JsonHelper.ToJson(httpGetRouterResponse)}");
            // 打乱顺序
            RandomGenerator.BreakRank(self.Info.Routers);
            self.WaitTenMinGetAllRouter().Coroutine(); // <<<<<<<<<<<<<<<<<<<< 
        }
        // 等10分钟再获取一次: 明明是 5 分钟，哪里有 10 分钟呢？
        public static async ETTask WaitTenMinGetAllRouter(this RouterAddressComponent self) {
            await TimerComponent.Instance.WaitAsync(5 * 60 * 1000);
            if (self.IsDisposed) 
                return;
            await self.GetAllRouter();
        }
        public static IPEndPoint GetAddress(this RouterAddressComponent self) {
            if (self.Info.Routers.Count == 0) 
                return null;
            string address = self.Info.Routers[self.RouterIndex++ % self.Info.Routers.Count];
            string[] ss = address.Split(':');
            IPAddress ipAddress = IPAddress.Parse(ss[0]);
            if (self.RouterManagerIPAddress.AddressFamily == AddressFamily.InterNetworkV6) { 
                ipAddress = ipAddress.MapToIPv6();
            }
            return new IPEndPoint(ipAddress, int.Parse(ss[1]));
        }
        public static IPEndPoint GetMatchAddress(this RouterAddressComponent self, string account) {
            int v = account.Mode(self.Info.Matchs.Count); // 它说，给它随机分配一个取模后的下编匹配服。。。
            string address = self.Info.Matchs[v];
            string[] ss = address.Split(':');
            IPAddress ipAddress = IPAddress.Parse(ss[0]);
            // if (self.IPAddress.AddressFamily == AddressFamily.InterNetworkV6) 
            //    ipAddress = ipAddress.MapToIPv6();
            return new IPEndPoint(ipAddress, int.Parse(ss[1]));
        }
        public static IPEndPoint GetRealmAddress(this RouterAddressComponent self, string account) {
            int v = account.Mode(self.Info.Realms.Count);
            string address = self.Info.Realms[v];
            string[] ss = address.Split(':');
            IPAddress ipAddress = IPAddress.Parse(ss[0]);
            // if (self.IPAddress.AddressFamily == AddressFamily.InterNetworkV6) 
            //    ipAddress = ipAddress.MapToIPv6();
            return new IPEndPoint(ipAddress, int.Parse(ss[1]));
        }
    }
}