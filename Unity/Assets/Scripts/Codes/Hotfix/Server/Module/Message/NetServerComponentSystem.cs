using System.Net;
namespace ET.Server {
    [FriendOf(typeof(NetServerComponent))] // 【服务端组件】：负责【服务端】的网络交互部分
    public static class NetServerComponentSystem {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<NetServerComponent, IPEndPoint> {
            protected override void Awake(NetServerComponent self, IPEndPoint address) {
                self.ServiceId = NetServices.Instance.AddService(new KService(address, ServiceType.Outer));
                NetServices.Instance.RegisterAcceptCallback(self.ServiceId, self.OnAccept); // 网络交互的几个回调事件
                NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead); // 三个回调：应该是模块的更底层才会触发调用的
                NetServices.Instance.RegisterErrorCallback(self.ServiceId, self.OnError);
            }
        }
        [ObjectSystem]
        public class NetKcpComponentDestroySystem: DestroySystem<NetServerComponent> {
            protected override void Destroy(NetServerComponent self) {
                NetServices.Instance.RemoveService(self.ServiceId);
            }
        }
        private static void OnError(this NetServerComponent self, long channelId, int error) {
            Session session = self.GetChild<Session>(channelId);
            if (session == null) return;
            session.Error = error;
            session.Dispose();
        }
        // 这个channelId是由CreateAcceptChannelId生成的
        private static void OnAccept(this NetServerComponent self, long channelId, IPEndPoint ipEndPoint) {
            // 【创建会话框】：当此【服务端】组件，接受了一个客户端，就建一个与接收的【客户端】的会话框
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = ipEndPoint;
// 只要不是这个鬼服BenchmarkServer：就加两个【服务端】的必要的，防盗防挂网不干事占带宽的盗贼，和检查客户端状况
            if (self.DomainScene().SceneType != SceneType.BenchmarkServer) { // 区分：同一功能，【服务端】的处理逻辑，与【客户端】的处理逻辑 
                // 挂上这个组件，5秒就会删除session，所以客户端验证完成要删除这个组件。该组件的作用就是防止外挂一直连接不发消息也不进行权限验证
                // C2G_LoginGateHandler: 【客户端】逻辑，客户端验证的地方
                session.AddComponent<SessionAcceptTimeoutComponent>(); // 上面原标注：【客户端验证】的逻辑，改天去找
                // 客户端连接，2秒检查一次recv消息，10秒没有消息则断开（与那个接收不到心跳包的客户端的连接）。【活宝妹就是一定要嫁给亲爱的表哥！！！】
                //【自己的理解】：【客户端】有心跳包告知服务端，各客户端的连接状况；【服务端】：同样有服务端此组件来检测说，哪个客户端掉线了？
                session.AddComponent<SessionIdleCheckerComponent>(); // 就是检查：【30 秒内】至少发送过消息，至少接收过消息，否则视为闲置回收
            }
        }
        // 从这里继续往前倒，去找哪里发布事件， message 是什么类型，什么内容？
        private static void OnRead(this NetServerComponent self, long channelId, long actorId, object message) {
            Session session = self.GetChild<Session>(channelId);
            if (session == null) return;
            session.LastRecvTime = TimeHelper.ClientNow();
            OpcodeHelper.LogMsg(self.DomainZone(), message);
            // 【发布事件】：服务端组件读到了消息。这个事件发布，事件的订阅者会收到通知，处理相应必要逻辑
            EventSystem.Instance.Publish(Root.Instance.Scene, new NetServerComponentOnRead() {Session = session, Message = message}); // <<<<<<<<<<<<<<<<<<<< 
        }
    }
}