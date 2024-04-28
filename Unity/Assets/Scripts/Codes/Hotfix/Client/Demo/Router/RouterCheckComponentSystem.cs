using System;
using System.Net;
namespace ET.Client {
	
    [ObjectSystem]
    public class RouterCheckComponentAwakeSystem: AwakeSystem<RouterCheckComponent> {
        protected override void Awake(RouterCheckComponent self) {
            CheckAsync(self).Coroutine();
        }

        private static async ETTask CheckAsync(RouterCheckComponent self) {
            Session session = self.GetParent<Session>();
            long instanceId = self.InstanceId;
            while (true) {
                if (self.InstanceId != instanceId) {
                    return;
                }
                await TimerComponent.Instance.WaitAsync(1000);
                
                if (self.InstanceId != instanceId) {
                    return;
                }
                long time = TimeHelper.ClientFrameTime();
                if (time - session.LastRecvTime < 7 * 1000) { // 每个、随机分配给客户端的【路由器】，客户端一般只连 7 秒左右
                    continue;
                }
				// 试着，重新、动态，再次获取一个【随机】路由器，完成相同的功能职责，那么这种随机动态性，浪费了网络带宽与流量，但安全防攻击
                try {
                    long sessionId = session.Id;
                    (uint localConn, uint remoteConn) = await NetServices.Instance.GetChannelConn(session.ServiceId, sessionId);
                    
                    IPEndPoint realAddress = self.GetParent<Session>().RemoteAddress;
                    Log.Info($"get recvLocalConn start: {self.ClientScene().Id} {realAddress} {localConn} {remoteConn}");
					// 重新，再试拿一个【随机动态】分配的【网络中的路由器】
                    (uint recvLocalConn, IPEndPoint routerAddress) = await RouterHelper.GetRouterAddress(self.ClientScene(), realAddress, localConn, remoteConn);
                    if (recvLocalConn == 0) {
                        Log.Error($"get recvLocalConn fail: {self.ClientScene().Id} {routerAddress} {realAddress} {localConn} {remoteConn}");
                        continue;
                    }

                    Log.Info($"get recvLocalConn ok: {self.ClientScene().Id} {routerAddress} {realAddress} {recvLocalConn} {localConn} {remoteConn}");
                    
                    session.LastRecvTime = TimeHelper.ClientNow();
					// 因为重新、动态、随机分配了，最可能的另一个路由器，那么更改现在【会话框】的本地【路由器】的地址，其它不变，远程仍是Reals 服
                    NetServices.Instance.ChangeAddress(session.ServiceId, sessionId, routerAddress);
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
    }
}