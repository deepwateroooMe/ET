using System.Net;
using System.Net.Sockets;
namespace ET.Server {
    [FriendOf(typeof(NetInnerComponent))]
    public static class NetInnerComponentSystem {
        [ObjectSystem]
        public class NetInnerComponentAwakeSystem: AwakeSystem<NetInnerComponent> {
            protected override void Awake(NetInnerComponent self) {
                NetInnerComponent.Instance = self;
                switch (self.InnerProtocol) {
                case NetworkProtocol.TCP: {
                    self.ServiceId = NetServices.Instance.AddService(new TService(AddressFamily.InterNetwork, ServiceType.Inner));
                    break;
                }
                case NetworkProtocol.KCP: {
                    self.ServiceId = NetServices.Instance.AddService(new KService(AddressFamily.InterNetwork, ServiceType.Inner));
                    break;
                }
                }
                NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead);
                NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError);
            }
        }
        [ObjectSystem]
        public class NetInnerComponentAwake1System: AwakeSystem<NetInnerComponent, IPEndPoint> {
            protected override void Awake(NetInnerComponent self, IPEndPoint address) {
                NetInnerComponent.Instance = self;
                switch (self.InnerProtocol) {
                case NetworkProtocol.TCP: {
                    self.ServiceId = NetServices.Instance.AddService(new TService(address, ServiceType.Inner));
                    break;
                }
                case NetworkProtocol.KCP: {
                    self.ServiceId = NetServices.Instance.AddService(new KService(address, ServiceType.Inner));
                    break;
                }
                }
                NetServices.Instance.RegisterAcceptCallback(self.ServiceId, self.OnAccept);
                NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead);
                NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError);
            }
        }
        [ObjectSystem]
        public class NetInnerComponentDestroySystem: DestroySystem<NetInnerComponent> {
            protected override void Destroy(NetInnerComponent self) {
                NetServices.Instance.RemoveService(self.ServiceId);
            }
        }
        private static void OnRead(this NetInnerComponent self, long channelId, long actorId, object message) {
            Session session = self.GetChild<Session>(channelId);
            if (session == null) {
                return;
            }
            session.LastRecvTime = TimeHelper.ClientFrameTime();
            self.HandleMessage(actorId, message);
        }
        // 这里，内网组件，处理内网消息看出，这些都重构成了事件机制，发布根场景内网组件读到消息事件
        public static void HandleMessage(this NetInnerComponent self, long actorId, object message) {
            EventSystem.Instance.Publish(Root.Instance.Scene, new NetInnerComponentOnRead() { ActorId = actorId, Message = message });
        }
        private static void OnError(this NetInnerComponent self, long channelId, int error) {
            Session session = self.GetChild<Session>(channelId);
            if (session == null) {
                return;
            }
            session.Error = error;
            session.Dispose();
        }
        // 这个channelId是由CreateAcceptChannelId生成的
        private static void OnAccept(this NetInnerComponent self, long channelId, IPEndPoint ipEndPoint) {
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = ipEndPoint;
            // session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);
        }
        private static Session CreateInner(this NetInnerComponent self, long channelId, IPEndPoint ipEndPoint) {
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = ipEndPoint;
            NetServices.Instance.CreateChannel(self.ServiceId, channelId, ipEndPoint);
            // session.AddComponent<InnerPingComponent>();
            // session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);
            return session;
        }
        // 去找一下：框架里是否有样例使用如下方法
        // 内网actor session，channelId是进程号。【自己的理解】：这些内网服务器间，或说重构的SceneType 间，有维护着会话框的，比如Realm 注册登录服与Gate 网关服等
        public static Session Get(this NetInnerComponent self, long channelId) {
            Session session = self.GetChild<Session>(channelId);
            if (session != null) { // 有已经创建过，就直接返回
                return session;
            } // 下面，还没创建过，就创建一个会话框
            IPEndPoint ipEndPoint = StartProcessConfigCategory.Instance.Get((int) channelId).InnerIPPort;
            session = self.CreateInner(channelId, ipEndPoint);
            return session;
        }
    }
}