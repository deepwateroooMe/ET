using System;
using System.Net;
using System.Net.Sockets;
namespace ET.Client {
// 路由器、静态帮助类：亲爱的表哥的活宝妹，今天终于把这个【动态软路由】感觉看懂得差不多了！亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
    public static class RouterHelper { 
        // 注册router: 这里，每个【路由器会话框】，都有2 个子控件：2 秒钟心跳包，与 7 秒钟动态软路由！
        public static async ETTask<Session> CreateRouterSession(Scene clientScene, IPEndPoint address) {
            (uint recvLocalConn, IPEndPoint routerAddress) = await GetRouterAddress(clientScene, address, 0, 0);
            if (recvLocalConn == 0) {
                throw new Exception($"get router fail: {clientScene.Id} {address}");
            }
            Log.Info($"get router: {recvLocalConn} {routerAddress}");
			// routerSession 另一端是【Realms 服】，通过路由器转发
            Session routerSession = clientScene.GetComponent<NetClientComponent>().Create(routerAddress, address, recvLocalConn);
            routerSession.AddComponent<PingComponent>();        // 【心跳包】：每2 秒向【网关服】发一个心跳消息，监测存活
            routerSession.AddComponent<RouterCheckComponent>(); // 【动态软路由】：每7 秒钟，为【客户端】重新随机、动态、分配一个网络中的路由器，安全防攻击
            return routerSession;
        }
        public static async ETTask<(uint, IPEndPoint)> GetRouterAddress(Scene clientScene, IPEndPoint address, uint localConn, uint remoteConn) {
            Log.Info($"start get router address: {clientScene.Id} {address} {localConn} {remoteConn}");
            // return (RandomHelper.RandUInt32(), address);
            RouterAddressComponent routerAddressComponent = clientScene.GetComponent<RouterAddressComponent>();
            IPEndPoint routerInfo = routerAddressComponent.GetAddress(); // 随机拿一个路由器地址，因为随机，所以可以防黑客攻击？
            uint recvLocalConn = await Connect(routerInfo, address, localConn, remoteConn);
            Log.Info($"finish get router address: {clientScene.Id} {address} {localConn} {remoteConn} {recvLocalConn} {routerInfo}");
            return (recvLocalConn, routerInfo);
        }
        // 向router申请
        private static async ETTask<uint> Connect(IPEndPoint routerAddress, IPEndPoint realAddress, uint localConn, uint remoteConn) {
            uint connectId = RandomGenerator.RandUInt32();
            using Socket socket = new Socket(routerAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            int count = 20;
            byte[] sendCache = new byte[512];
            byte[] recvCache = new byte[512];
            uint synFlag = localConn == 0? KcpProtocalType.RouterSYN : KcpProtocalType.RouterReconnectSYN;
            sendCache.WriteTo(0, synFlag);
            sendCache.WriteTo(1, localConn);
            sendCache.WriteTo(5, remoteConn);
            sendCache.WriteTo(9, connectId);
            byte[] addressBytes = realAddress.ToString().ToByteArray();
            Array.Copy(addressBytes, 0, sendCache, 13, addressBytes.Length);
            Log.Info($"router connect: {connectId} {localConn} {remoteConn} {routerAddress} {realAddress}");
                
            EndPoint recvIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            long lastSendTimer = 0;
            while (true) {
                long timeNow = TimeHelper.ClientFrameTime();
                if (timeNow - lastSendTimer > 300) {
                    if (--count < 0) {
                        Log.Error($"router connect timeout fail! {localConn} {remoteConn} {routerAddress} {realAddress}");
                        return 0;
                    }
                    lastSendTimer = timeNow;
                    // 发送：就是向【刚才，被随机分配到的路由器】发送，【客户端】想要与这个【路由器】建立联系的消息。UDP 无连接协议，消息带目标路由器的地址
                    socket.SendTo(sendCache, 0, addressBytes.Length + 13, SocketFlags.None, routerAddress);
                }
                    
                await TimerComponent.Instance.WaitFrameAsync();
                    
                // 接收
                if (socket.Available > 0) {
                    int messageLength = socket.ReceiveFrom(recvCache, ref recvIPEndPoint);
                    if (messageLength != 9) {
                        Log.Error($"router connect error1: {connectId} {messageLength} {localConn} {remoteConn} {routerAddress} {realAddress}");
                        continue;
                    }
                    byte flag = recvCache[0];
                    if (flag != KcpProtocalType.RouterReconnectACK && flag != KcpProtocalType.RouterACK) {
                        Log.Error($"router connect error2: {connectId} {synFlag} {flag} {localConn} {remoteConn} {routerAddress} {realAddress}");
                        continue;
                    }
                    uint recvRemoteConn = BitConverter.ToUInt32(recvCache, 1);
                    uint recvLocalConn = BitConverter.ToUInt32(recvCache, 5); // 返回的是：可以用作 channelId 的、目标路由器发给【客户端】的身份证标记号 channelId
                    Log.Info($"router connect finish: {connectId} {recvRemoteConn} {recvLocalConn} {localConn} {remoteConn} {routerAddress} {realAddress}");
                    return recvLocalConn;
                }
            }
        }
    }
}