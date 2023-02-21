using System.Net;
using System.Net.Sockets;
namespace ET.Server {
    [FriendOf(typeof(NetInnerComponent))]
    public static class NetInnerComponentSystem {
        [ObjectSystem]
        public class NetInnerComponentAwakeSystem: AwakeSystem<NetInnerComponent> {
            protected override void Awake(NetInnerComponent self) {
                NetInnerComponent.Instance = self;
                switch (self.InnerProtocol) { // 根据服务机制的不同： KCP TCP WEBSOCKET
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
            protected override void Awake(NetInnerComponent self, IPEndPoint address) { // 传入一个终端地址，是终端吗？是
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
                NetServices.Instance.RegisterAcceptCallback(self.ServiceId, self.OnAccept); // 是终端，当连接上时的回调
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
            self.HandleMessage(actorId, message); // 就会回调到，先前注册过的回调接口。先前注册的过程应该可以由 Session ＝＝》 T/KService ＝＝》 T/KChannel 一步步的封装往下回调执行
        }
        public static void HandleMessage(this NetInnerComponent self, long actorId, object message) {
            // 这里的 Publish: 也就是触发一次回调，可以抛异常
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
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId); // 去抓壮丁，抓（从对象池中取，或生成新的），抓一个会话框
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
        // 内网actor session，channelId是进程号
        public static Session Get(this NetInnerComponent self, long channelId) {
            Session session = self.GetChild<Session>(channelId);
            if (session != null) {
                return session;
            }
            IPEndPoint ipEndPoint = StartProcessConfigCategory.Instance.Get((int) channelId).InnerIPPort;
            session = self.CreateInner(channelId, ipEndPoint);
            return session;
        }
    }
}