using System.Net;
using System.Net.Sockets;
namespace ET.Client {
	// 【客户端网络组件】：
    [FriendOf(typeof(NetClientComponent))]
    public static class NetClientComponentSystem {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<NetClientComponent, AddressFamily> {
            protected override void Awake(NetClientComponent self, AddressFamily addressFamily) {
                self.ServiceId = NetServices.Instance.AddService(new KService(addressFamily, ServiceType.Outer));
                NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead);
                NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError);
            }
        }
        [ObjectSystem]
        public class DestroySystem: DestroySystem<NetClientComponent> {
            protected override void Destroy(NetClientComponent self) {
                NetServices.Instance.RemoveService(self.ServiceId);
            }
        }
        private static void OnRead(this NetClientComponent self, long channelId, long actorId, object message) { // 客户端、讲到消息的逻辑
            Session session = self.GetChild<Session>(channelId);
            if (session == null) {
                return;
            }
            session.LastRecvTime = TimeHelper.ClientNow(); // 更新：信道上会话框、最后活动时间
            OpcodeHelper.LogMsg(self.DomainZone(), message);
			// 【客户端】读到消息：要去找事件的订阅者，去查看订阅者的后续逻辑.【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
			// 客户端热更域：相对顶层，订阅了事件 NetClientComponentOnReadEvent
            EventSystem.Instance.Publish(Root.Instance.Scene, new NetClientComponentOnRead() {Session = session, Message = message});
        }
        private static void OnError(this NetClientComponent self, long channelId, int error) {
            Session session = self.GetChild<Session>(channelId);
            if (session == null) {
                return;
            }
            session.Error = error;
            session.Dispose();
        }
        public static Session Create(this NetClientComponent self, IPEndPoint realIPEndPoint) {
            long channelId = NetServices.Instance.CreateConnectChannelId();
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = realIPEndPoint;
            if (self.Domain.SceneType != SceneType.Benchmark) {
                session.AddComponent<SessionIdleCheckerComponent>();
            }
            NetServices.Instance.CreateChannel(self.ServiceId, session.Id, realIPEndPoint);
            return session;
        }
        public static Session Create(this NetClientComponent self, IPEndPoint routerIPEndPoint, IPEndPoint realIPEndPoint, uint localConn) {
            long channelId = localConn;
			// 刚才不是不懂 Session.Domain.SceneType 吗？这里就是【客户端】场景的场景类型呀。因为子Domain 继承父控件的Domain, 就是客户端场景的类型
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = realIPEndPoint; // 【会话框】远程是【Realms 服】。所以这里看【客户端】与【Realms 服】是通过【路由器】转发的、完全随机、动态、安全防攻击
            if (self.Domain.SceneType != SceneType.Benchmark) {
                session.AddComponent<SessionIdleCheckerComponent>(); // 【会话框】30 秒不活动、超时自检测机制
            }
			// 下面，CreateChannel() 就写好了，【会话框】的本地？成【路由器】地址与端口等，借助路由器中转，建立起客户端与Realms 服间的通信连接
            NetServices.Instance.CreateChannel(self.ServiceId, session.Id, routerIPEndPoint);
            return session;
        }
    }
}