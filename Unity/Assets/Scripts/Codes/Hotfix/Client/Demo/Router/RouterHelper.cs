using System;
using System.Net;
using System.Net.Sockets;
namespace ET.Client {
    // 【路由器帮助类】：现有理解，怎么感觉更像是帮助建立【网关服】同【其它服】的通信会话框？把这个类看懂
    // 这个类，框架开发者的原始标记，都看不懂。能够看懂、想出自己理解的一个大概轮括。需要改天准备了必要基础知识后再读一遍
    // 框架以前版本，【客户端】只与【网关服】通信，网关服是它所管辖小区里所有【客户端】的通信代理。
    // 框架重构后的现在版本说，不要什么【网关服】代理了，【客户端】通过客户端所在的路由系统下的【路由总管？】来收发消息。这里【路由总管】感觉功能上，相当于先前随机分配给当前【客户端】的【网关服】。不知道这么理解对不对，记下，再多想一想
    public static class RouterHelper {
        // 【注册router】：什么叫注册 router? 为什么我觉得是在建会话框？这个方法没能看完。它是为当前【客户端场景】添加必备路由网络通信功能模块。注意添加的几个组件
        public static async ETTask<Session> CreateRouterSession(Scene clientScene, IPEndPoint address) {
// 拿客户端场景路由器地址：
            (uint recvLocalConn, IPEndPoint routerAddress) = await GetRouterAddress(clientScene, address, 0, 0); 
            if (recvLocalConn == 0) {
                throw new Exception($"get router fail: {clientScene.Id} {address}");
            }
            Log.Info($"get router: {recvLocalConn} {routerAddress}");
            Session routerSession = clientScene.GetComponent<NetClientComponent>().Create(routerAddress, address, recvLocalConn); // 直接建立了【客户端】会话框
            // 前面想到，这个路由组件，功能上相当于先前的【网关服】
            routerSession.AddComponent<PingComponent>(); // 路由组件：它需要心跳包给服务端知道，这个组件，是否掉线了？
            routerSession.AddComponent<RouterCheckComponent>(); 
            return routerSession;
        }
        // 不去细看关于RounterAddressComponent 里的这个方法了。感觉关于Rounter 的路由原理，我可能这个模块缺了一点儿基础知识，所以看得吃力看不懂。但我现在不需要去搞懂路由原理，跳过
        public static async ETTask<(uint, IPEndPoint)> GetRouterAddress(Scene clientScene, IPEndPoint address, uint localConn, uint remoteConn) {
            Log.Info($"start get router address: {clientScene.Id} {address} {localConn} {remoteConn}");
            // return (RandomHelper.RandUInt32(), address);
            RouterAddressComponent routerAddressComponent = clientScene.GetComponent<RouterAddressComponent>(); // 它就是在 LoginHelper 里添加的呀
// 这里得看懂：【局域网内网下具备对外网收发消息的管理总管的地址？现感觉这里写反了呀，是局域网内网下客户端在路由系统中被分配的端口】，它的路由器的端口，是一定变化了的？
            IPEndPoint routerInfo = routerAddressComponent.GetAddress(); 
// 就是说，【局域网内网内部，客户端接收消息的专用连接】：从局域网内网下具备对外网收发消息的管理总管，接收消息的局域网内内网连接
            uint recvLocalConn = await Connect(routerInfo, address, localConn, remoteConn); 
            Log.Info($"finish get router address: {clientScene.Id} {address} {localConn} {remoteConn} {recvLocalConn} {routerInfo}");
            return (recvLocalConn, routerInfo);
        }
        // 【向router申请】：应该是，使用了路由器的，当前【客户端】与远程【服务端】，实际路由建立会话框的，实现逻辑。是真正重新建立起一个新的通信信道的逻辑
        private static async ETTask<uint> Connect(IPEndPoint routerAddress, IPEndPoint realAddress, uint localConn, uint remoteConn) {
            uint connectId = RandomGenerator.RandUInt32(); // 随机生成一个：身份证号。。
            using Socket socket = new Socket(routerAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp); // 建立一个通话信道
            int count = 20; // 20 是什么意思呢：一个信道，最多同时（不同时？）发20 条消息？
            byte[] sendCache = new byte[512]; // 【发送】与【接收】缓存区
            byte[] recvCache = new byte[512];
            uint synFlag = localConn == 0? KcpProtocalType.RouterSYN : KcpProtocalType.RouterReconnectSYN; // 消息的同步机制？
            // 消息头相关的：一堆杂七杂八的？
            sendCache.WriteTo(0, synFlag);
            sendCache.WriteTo(1, localConn);
            sendCache.WriteTo(5, remoteConn);
            sendCache.WriteTo(9, connectId);
            byte[] addressBytes = realAddress.ToString().ToByteArray();
            Array.Copy(addressBytes, 0, sendCache, 13, addressBytes.Length); // 复制消息头
            Log.Info($"router connect: {connectId} {localConn} {remoteConn} {routerAddress} {realAddress}");
                
            EndPoint recvIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            long lastSendTimer = 0;
            while (true) { // 无限循环：信道的专职工作，周而复始。。。
                long timeNow = TimeHelper.ClientFrameTime();
                if (timeNow - lastSendTimer > 300) { // 按时间算，300 毫秒
                    if (--count < 0) {
                        Log.Error($"router connect timeout fail! {localConn} {remoteConn} {routerAddress} {realAddress}");
                        return 0;
                    }
                    lastSendTimer = timeNow;
                    // 发送：从当前信道，将消息发出去
                    socket.SendTo(sendCache, 0, addressBytes.Length + 13, SocketFlags.None, routerAddress);
                }
// 等待桢同步？时间组件管理类说等1 毫秒（还是等1 毫秒呢，感觉是1 毫秒），应该也就是（双端1 秒1 桢？桢率太少，60fps 每秒60 桢）等待这一个异步线程的操作同步到主线程上去？
                await TimerComponent.Instance.WaitFrameAsync(); 
                // 【接收：】这里KCP 路由器收消息的原理，感觉不太懂，改天再读
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
                    uint recvLocalConn = BitConverter.ToUInt32(recvCache, 5);
                    Log.Info($"router connect finish: {connectId} {recvRemoteConn} {recvLocalConn} {localConn} {remoteConn} {routerAddress} {realAddress}");
                    return recvLocalConn;
                }
            }
        }
    }
}