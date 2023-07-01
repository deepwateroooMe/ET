using System.Net;
using System.Net.Sockets;
namespace ET.Client { 
    [FriendOf(typeof(NetClientComponent))] // 把这个【网络客户端】组件的主要笔记要点，再快速写一遍
    public static class NetClientComponentSystem {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<NetClientComponent, AddressFamily> {
            protected override void Awake(NetClientComponent self, AddressFamily addressFamily) { // 需要什么样的参数，就传什么样的参数
                self.ServiceId = NetServices.Instance.AddService(new KService(addressFamily, ServiceType.Outer)); // 开启了与这个客户端的网络服务
                NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead); // 注册订阅【读】网络消息事件，应该是从网络服务的服务端订阅
                NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError); // 注册订阅【出错】事件
            }
        }
        [ObjectSystem]
        public class DestroySystem: DestroySystem<NetClientComponent> {
            protected override void Destroy(NetClientComponent self) {
                NetServices.Instance.RemoveService(self.ServiceId); // 直接移除这个网络服务
            }
        }
        private static void OnRead(this NetClientComponent self, long channelId, long actorId, object message) {
            Session session = self.GetChild<Session>(channelId); // 拿：相应的会话框
            if (session == null) return; // 空：直接返回
            session.LastRecvTime = TimeHelper.ClientNow();
            OpcodeHelper.LogMsg(self.DomainZone(), message);
// 发布事件：事件的接收者，应该是【客户端】的Session 层面的进一步读取消息内容（内存流上读消息？），改天再去细看。
            EventSystem.Instance.Publish(Root.Instance.Scene, new NetClientComponentOnRead() {Session = session, Message = message}); 
        }
        private static void OnError(this NetClientComponent self, long channelId, int error) {
            Session session = self.GetChild<Session>(channelId); // 同样，先去拿会话框：因为这些异步网络的消息传递，都是建立在一个个会话框的基础上的
            if (session == null) return; // 空：直接返回 
            session.Error = error;
            session.Dispose();
        }
        public static Session Create(this NetClientComponent self, IPEndPoint realIPEndPoint) {
            long channelId = NetServices.Instance.CreateConnectChannelId();
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId); // 创建必要的会话框，方便交通
            session.RemoteAddress = realIPEndPoint;
            if (self.DomainScene().SceneType != SceneType.Benchmark) 
                session.AddComponent<SessionIdleCheckerComponent>(); // 不知道这个是干什么的，改天再看
            NetServices.Instance.CreateChannel(self.ServiceId, session.Id, realIPEndPoint); // 创建信道
            return session;
        }
        public static Session Create(this NetClientComponent self, IPEndPoint routerIPEndPoint, IPEndPoint realIPEndPoint, uint localConn) {
            long channelId = localConn;
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = realIPEndPoint;
            if (self.DomainScene().SceneType != SceneType.Benchmark) 
                session.AddComponent<SessionIdleCheckerComponent>();
            NetServices.Instance.CreateChannel(self.ServiceId, session.Id, routerIPEndPoint);
            return session;
        }
    }
}