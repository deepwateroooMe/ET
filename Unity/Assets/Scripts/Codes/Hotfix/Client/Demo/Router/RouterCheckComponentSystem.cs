using System;
using System.Net;
namespace ET.Client {
    [ObjectSystem]
    public class RouterCheckComponentAwakeSystem: AwakeSystem<RouterCheckComponent> {
        protected override void Awake(RouterCheckComponent self) {
            CheckAsync(self).Coroutine();
        }
        private static async ETTask CheckAsync(RouterCheckComponent self) {
            Session session = self.GetParent<Session>(); // 路由器专用会话框
            long instanceId = self.InstanceId;
            while (true) {
                if (self.InstanceId != instanceId) return;
                await TimerComponent.Instance.WaitAsync(1000);
                if (self.InstanceId != instanceId) return;
                long time = TimeHelper.ClientFrameTime();
// 【路由组件】：7 秒钟检查（这里不止检查，重新建新的信道会话框？）一次，当前路由器是否掉线了？如同先前心跳包，心跳包2 秒发条最简消息。。
                if (time - session.LastRecvTime < 7 * 1000) continue;
                try {
                    long sessionId = session.Id;
                    // 【异步方法】：网络异步调用，去拿当前客户端的网络服务的信道信息（一个信道连两个端点：一个本地端口，一个远程端口）
                    // 下面一行的疑问：当去拿【当前】会话框的信道，两端端口信息，拿到的是现信道两端口的【网络异步读取】到的现存信道信息
                    (uint localConn, uint remoteConn) = await NetServices.Instance.GetChannelConn(session.ServiceId, sessionId);
                    // 【去找】：当前组件添加的地方，它的【会话框】有什么特殊的地方吗？【会话框的远程地址】：RouterHelper.cs 类里面添加的，为【】添加的
                    IPEndPoint realAddress = self.GetParent<Session>().RemoteAddress; // 局域网内网下具备对外网收发消息的管理总管的地址，是当前会话框的远程地址，不变，会再用
                    Log.Info($"get recvLocalConn start: {self.ClientScene().Id} {realAddress} {localConn} {remoteConn}");
                    // RouterHelper.GetRouterAddress(): 这个方法里，感觉是重新、重建立了新的、更新了通信信道会话框，而不仅仅是每 7 秒检查先前会话框是否仍有效，或是连接着的有效状态 
                    (uint recvLocalConn, IPEndPoint routerAddress) = await RouterHelper.GetRouterAddress(self.ClientScene(), realAddress, localConn, remoteConn);
                    if (recvLocalConn == 0) {
                        Log.Error($"get recvLocalConn fail: {self.ClientScene().Id} {routerAddress} {realAddress} {localConn} {remoteConn}");
                        continue;
                    }
                    Log.Info($"get recvLocalConn ok: {self.ClientScene().Id} {routerAddress} {realAddress} {recvLocalConn} {localConn} {remoteConn}");
                    session.LastRecvTime = TimeHelper.ClientNow();
                    NetServices.Instance.ChangeAddress(session.ServiceId, sessionId, routerAddress); // 对新通信信道的网络服务变量参数，更新到管理单例类
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
    }
}