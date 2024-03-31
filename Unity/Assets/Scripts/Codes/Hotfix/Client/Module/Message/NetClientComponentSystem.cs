﻿using System.Net;
using System.Net.Sockets;
namespace ET.Client {
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
			// 【客户端】读到消息：接下来怎么样？要去找事件的订阅者，去查看订阅者的后续逻辑。明天上午再接着看一遍。
			// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
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
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = realIPEndPoint;
            if (self.Domain.SceneType != SceneType.Benchmark) {
                session.AddComponent<SessionIdleCheckerComponent>();
            }
            NetServices.Instance.CreateChannel(self.ServiceId, session.Id, routerIPEndPoint);
            return session;
        }
    }
}