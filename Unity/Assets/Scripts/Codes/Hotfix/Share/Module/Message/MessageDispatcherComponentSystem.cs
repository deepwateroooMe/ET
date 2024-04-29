using System;
using System.Collections.Generic;
namespace ET {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
    // 消息分发组件、静态帮助类：
    [FriendOf(typeof(MessageDispatcherComponent))]
    public static class MessageDispatcherComponentHelper {
        [ObjectSystem]
        public class MessageDispatcherComponentAwakeSystem: AwakeSystem<MessageDispatcherComponent> {
            protected override void Awake(MessageDispatcherComponent self) {
                MessageDispatcherComponent.Instance = self;
                self.Load();
            }
        }
        [ObjectSystem]
        public class MessageDispatcherComponentLoadSystem: LoadSystem<MessageDispatcherComponent> {
            protected override void Load(MessageDispatcherComponent self) {
                self.Load();
            }
        }
        [ObjectSystem]
        public class MessageDispatcherComponentDestroySystem: DestroySystem<MessageDispatcherComponent> {
            protected override void Destroy(MessageDispatcherComponent self) {
                MessageDispatcherComponent.Instance = null;
                self.Handlers.Clear();
            }
        }
        private static void Load(this MessageDispatcherComponent self) {
            self.Handlers.Clear();
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (MessageHandlerAttribute));
            foreach (Type type in types) {
                IMHandler iMHandler = Activator.CreateInstance(type) as IMHandler; // 创建实例，进程启动时
                if (iMHandler == null) {
                    Log.Error($"message handle {type.Name} 需要继承 IMHandler");
                    continue;
                }
                object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
                foreach (object attr in attrs) {
                    MessageHandlerAttribute messageHandlerAttribute = attr as MessageHandlerAttribute;
                    Type messageType = iMHandler.GetMessageType(); // 获取、各类来往消息的、类型——唯一标识
                    ushort opcode = NetServices.Instance.GetOpcode(messageType); // 消息网络操作码——唯一标识
                    if (opcode == 0) {
                        Log.Error($"消息opcode为0: {messageType.Name}"); 
                        continue;
                    }
                    MessageDispatcherInfo messageDispatcherInfo = new (messageHandlerAttribute.SceneType, iMHandler); // 【场景、消息处理器】
                    self.RegisterHandler(opcode, messageDispatcherInfo); // 进程上、消息派发器：【网络操作码：【场景、消息处理器】＋【场景、消息处理器】。。。】
                }
            }
        }
        private static void RegisterHandler(this MessageDispatcherComponent self, ushort opcode, MessageDispatcherInfo handler) {
            if (!self.Handlers.ContainsKey(opcode)) {
                self.Handlers.Add(opcode, new List<MessageDispatcherInfo>()); // 同一【网络消息操作码】，可有多个不同的、消息处理场景
            }
            self.Handlers[opcode].Add(handler);
        }
		// 逻辑：进程收到消息后，下放到场景，由场景去实现具体功能逻辑
        public static void Handle(this MessageDispatcherComponent self, Session session, object message) { 
            List<MessageDispatcherInfo> actions;
            ushort opcode = NetServices.Instance.GetOpcode(message.GetType());
            if (!self.Handlers.TryGetValue(opcode, out actions)) {
                Log.Error($"消息没有处理: {opcode} {message}");
                return;
            }
			// 【会话框】的场景类型，【客户端】的场景类型，一般代表的是，分配给这个客户端的【网关服】所属区相关标记
            SceneType sceneType = session.Domain.SceneType; // 特定场景：特定场景下，可以存在多个不同的、针对同类消息的 MessageDispatcherInfo
			// 理解实际用例：会话框上、下来的一个消息、消息对应的同一场景下，可能存在、多个不同的【消息处理器】：
			// 考虑先前它们说过的，随时备份出一个分服分身来，同一场景可以有N 个备份与分服，每个备份与分身上，是否会各备一个自己分服分身上的【消息处理器】呢？这么想是合理的！
            foreach (MessageDispatcherInfo ev in actions) {
                if (!ev.SceneType.HasSameFlag(sceneType)) { // 不是这个场景
                    continue;
                }
				// 那么：会话框上、同一消息、所固定的场景下，因为分服分身的存在，可以有多个不同的【消息处理器】。觉得这么想，是对的！
				// 功能逻辑不完整：分服分身是减压的，每个都运行，除非某些的可跳过，如多进程安卓应用，不要每个进程都重启一遍应用。可是这把【分服分身】的逻辑无限复杂化了，可能想得不对！
				// 同一场景类型，多个【消息处理器】：同一【进程】上、同样场景类型的分线【像是还有点儿逻辑意义】、分身分服【同一进程上整多个有用吗】？备份？
                try { 
                    ev.IMHandler.Handle(session, message); // 每一个消息处理器的逻辑，感觉是懂的；就是同一类消息，同时存在多个不同的消息处理器？想不通
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
    }
}