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
        // 内网actor session，channelId是进程号【源】：
		// 就是想拿，发【内网消息】时，去往某个内网服务器的【会话框——本进程，与、目标进程，之间的通信会话框】，没有就创建一个
		// 【内网组件】：添加在内网服务器各进程上
        public static Session Get(this NetInnerComponent self, long channelId) {
            Session session = self.GetChild<Session>(channelId);
            if (session != null) {
                return session;
            }
            IPEndPoint ipEndPoint = StartProcessConfigCategory.Instance.Get((int) channelId).InnerIPPort; // 远和：目标进程的、对外通信开放端口
            session = self.CreateInner(channelId, ipEndPoint); // 创建【内网、2进程间的会话框】
            return session;
        }
    }
} // 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！