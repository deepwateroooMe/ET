using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ET.Client {

// 路由组件系统:
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

        private static async ETTask GetAllRouter(this RouterAddressComponent self) {
            string url = $"http:// {self.RouterManagerHost}:{self.RouterManagerPort}/get_router?v={RandomGenerator.RandUInt32()}";
            Log.Debug($"start get router info: {url}");
            string routerInfo = await HttpClientHelper.Get(url);
            Log.Debug($"recv router info: {routerInfo}");
            HttpGetRouterResponse httpGetRouterResponse = JsonHelper.FromJson<HttpGetRouterResponse>(routerInfo);
            self.Info = httpGetRouterResponse;
            Log.Debug($"start get router info finish: {JsonHelper.ToJson(httpGetRouterResponse)}");
            
            // 打乱顺序
            RandomGenerator.BreakRank(self.Info.Routers);
            
            self.WaitTenMinGetAllRouter().Coroutine();
        }
        // 等10分钟再获取一次: 这里的十分钟就是说,过了十分钟无响应不再合法的,会被去掉?
        public static async ETTask WaitTenMinGetAllRouter(this RouterAddressComponent self) {
            await TimerComponent.Instance.WaitAsync(5 * 60 * 1000); // 5 分钟
            if (self.IsDisposed) {
                return;
            }
            await self.GetAllRouter(); // <<<<<<<<<< 这里又调用了一次,所以总共算是等了十分钟
        }

        public static IPEndPoint GetAddress(this RouterAddressComponent self) {
            if (self.Info.Routers.Count == 0) {
                return null;
            }
            string address = self.Info.Routers[self.RouterIndex++ % self.Info.Routers.Count];
            string[] ss = address.Split(':');
            IPAddress ipAddress = IPAddress.Parse(ss[0]);
            if (self.RouterManagerIPAddress.AddressFamily == AddressFamily.InterNetworkV6) { 
                ipAddress = ipAddress.MapToIPv6();
            }
            return new IPEndPoint(ipAddress, int.Parse(ss[1]));
        }
        
        public static IPEndPoint GetRealmAddress(this RouterAddressComponent self, string account) {
            int v = account.Mode(self.Info.Realms.Count);
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
}