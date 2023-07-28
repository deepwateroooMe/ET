using System.Net;
namespace ET.Server {
    [FriendOf(typeof(NetServerComponent))] // 【服务端组件】：负责【服务端】的网络交互部分
    public static class NetServerComponentSystem {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<NetServerComponent, IPEndPoint> {
            protected override void Awake(NetServerComponent self, IPEndPoint address) {
                // 当一个【场景启动】起来，向NetServices 单例总管，注册三大回调。当向总管注册三回调的时候，它，不是相当于是总管的【客户端】？
                // 更像是，【单线程多进程架构】里，异步网络线程，向主线程，注册三大回调
                self.ServiceId = NetServices.Instance.AddService(new KService(address, ServiceType.Outer));
                NetServices.Instance.RegisterAcceptCallback(self.ServiceId, self.OnAccept); // 三个回调 
                NetServices.Instance.RegisterReadCallback(self.ServiceId, self.OnRead);
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
            session.RemoteAddress = ipEndPoint; // 【当前会话框】，它的远程是，一个【客户端】的IP 地址
            if (self.DomainScene().SceneType != SceneType.BenchmarkServer) { // 区分：同一功能，【服务端】的处理逻辑，与【客户端】的处理逻辑 
                // 挂上这个组件，5秒就会删除session，所以客户端验证完成要删除这个组件。该组件的作用就是防止外挂一直连接不发消息也不进行权限验证
                // 【客户端】逻辑，客户端验证的地方：C2G_LoginGateHandler: 这个例子，当前自称服务端组件，才更像【客户端】呢
                session.AddComponent<SessionAcceptTimeoutComponent>(); // 上面原标注：【客户端验证】的逻辑
                // 客户端连接，2秒检查一次recv消息，10秒没有消息则断开（与那个，此服务端接收不到心跳包的客户端，的连接）。【活宝妹就是一定要嫁给亲爱的表哥！！！】
                //【自己的理解】：【客户端】有心跳包告知服务端，各客户端的连接状况；【服务端】：同样有服务端此组件来检测说，哪个客户端掉线了？
                session.AddComponent<SessionIdleCheckerComponent>(); // 检查【会话框】是否有效：【30 秒内】至少发送过消息，至少接收过消息，否则视为闲置回收
            }
        }
        // 从这里继续往前倒，去找哪里发布事件， message 是什么类型，什么内容？【这里就是不懂】
        private static void OnRead(this NetServerComponent self, long channelId, long actorId, object message) {
            Session session = self.GetChild<Session>(channelId); // 从当前【服务端】所管理的所有会话框（连接的所有客户端）里，找到对应的 session(客户端 )
            if (session == null) return;
            session.LastRecvTime = TimeHelper.ClientNow();
            OpcodeHelper.LogMsg(self.DomainZone(), message);
            // 【发布事件】：服务端组件读到了消息。这里读到消息：是读到【主线程】回调回来的消息（把当前场景，视作网络异步线程？）
            EventSystem.Instance.Publish(Root.Instance.Scene, new NetServerComponentOnRead() {Session = session, Message = message});
            // 【事件的订阅者】：进程上的 NetServerComponentOnReadEvent
            // 进程被【1-N】个不同场景共享，是更底层。这里发出事件，【消息的接收者】，可能在【同一进程其它场景】，也可能在【其它进程】其它场景
            // 这里，【事件发布】到【事件订阅者】的过程，更像是，由某个场景，到【1-N】个可能场景所共享的，更底层的对应核，的过程
            // 【1-N】个可能场景所共享的，更底层的【这一个对应核】：订阅了事件。处理逻辑：是本进程的场景，接收场景去处理；不同进程？ rpc 。。。
        }
    }
}