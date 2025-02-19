﻿using System.Net;
using System.Net.Sockets;
namespace ET.Server {
    
    [FriendOf(typeof(NetInnerComponent))] // 为什么会感觉这个类看丢了？因为它的功能不熟悉。。。过目即忘。。
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
				// 向【单例模式】的同【网络模块主线程】注册三大回调事件
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

        // 从这里，再往前找，什么时候回调OnRead(), 私有方法 
        private static void OnRead(this NetInnerComponent self, long channelId, long actorId, object message) {
            Session session = self.GetChild<Session>(channelId);
            if (session == null) {
                return;
            }
            session.LastRecvTime = TimeHelper.ClientFrameTime();
            self.HandleMessage(actorId, message); // <<<<<<<<<<<<<<<<<<<< 调用下面的方法
        }
        // 事件发布：【内网】（从远程跨进程）读到消息（【返回消息】【普通消息】等，来自本进程的【发送消息】？）
        public static void HandleMessage(this NetInnerComponent self, long actorId, object message) { // 上面，本类内部方法调用的
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
        // 这个channelId是由CreateAcceptChannelId生成的：【这里去找一下】，
        private static void OnAccept(this NetInnerComponent self, long channelId, IPEndPoint ipEndPoint) { // 【网络服务端】告诉【客户端】说：建立了连接
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId); // 【内网组件】：创建子控件【会话框】内网通讯
            session.RemoteAddress = ipEndPoint;
            // session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);  // 这句是，它原本就 comment 掉的？亲爱的表哥的活宝妹，以为自己不小心弄的。。 
        }
        private static Session CreateInner(this NetInnerComponent self, long channelId, IPEndPoint ipEndPoint) {
            Session session = self.AddChildWithId<Session, int>(channelId, self.ServiceId);
            session.RemoteAddress = ipEndPoint;
            NetServices.Instance.CreateChannel(self.ServiceId, channelId, ipEndPoint);
            // session.AddComponent<InnerPingComponent>();
            // session.AddComponent<SessionIdleCheckerComponent, int, int, int>(NetThreadComponent.checkInteral, NetThreadComponent.recvMaxIdleTime, NetThreadComponent.sendMaxIdleTime);
            return session;
        }
        // 内网actor session，channelId是进程号。【自己的理解】：这些内网服务器间，或说重构的SceneType 间，有维护着会话框的，比如Realm 注册登录服与Gate 网关服等
        public static Session Get(this NetInnerComponent self, long channelId) { // 这里是，自己搞不清楚，IP 地址与端口，与 channelId 的关系，是什么，如何转化
            Session session = self.GetChild<Session>(channelId);
            if (session != null) { // 有已经创建过，就直接返回
                return session;
            } // 下面，还没创建过，就创建一个会话框
            IPEndPoint ipEndPoint = StartProcessConfigCategory.Instance.Get((int) channelId).InnerIPPort; // 这里拿的是：内网另一进程，可用来接收消息的端口
            session = self.CreateInner(channelId, ipEndPoint); // 当前服务器，与内网其它服务器的接收消息端口，建立会话框
            return session;
        }
    }
}