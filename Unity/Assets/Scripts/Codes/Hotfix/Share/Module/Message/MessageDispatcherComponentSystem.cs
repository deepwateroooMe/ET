using System;
using System.Collections.Generic;
namespace ET {

    // 消息分发组件, 它说是个帮助类，是使用标签系，加载时自动扫描标签实例化出来的帮助类
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
                IMHandler iMHandler = Activator.CreateInstance(type) as IMHandler;
                if (iMHandler == null) {
                    Log.Error($"message handle {type.Name} 需要继承 IMHandler");
                    continue;
                }
                object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
                foreach (object attr in attrs) {
                    MessageHandlerAttribute messageHandlerAttribute = attr as MessageHandlerAttribute;
                    
                    Type messageType = iMHandler.GetMessageType();
                    
                    ushort opcode = NetServices.Instance.GetOpcode(messageType); // 这里相对、理解上的困难是：感觉无法把OpCode 网络操作码与消息类型，从概念上连接起来
                    if (opcode == 0) {
                        Log.Error($"消息opcode为0: {messageType.Name}");
                        continue;
                    }
                    MessageDispatcherInfo messageDispatcherInfo = new (messageHandlerAttribute.SceneType, iMHandler);
                    self.RegisterHandler(opcode, messageDispatcherInfo);
                }
            }
        }
        private static void RegisterHandler(this MessageDispatcherComponent self, ushort opcode, MessageDispatcherInfo handler) {
            if (!self.Handlers.ContainsKey(opcode)) {
                self.Handlers.Add(opcode, new List<MessageDispatcherInfo>());
            }
            self.Handlers[opcode].Add(handler);
        }
        public static void Handle(this MessageDispatcherComponent self, Session session, object message) {
            List<MessageDispatcherInfo> actions;
            ushort opcode = NetServices.Instance.GetOpcode(message.GetType());
            if (!self.Handlers.TryGetValue(opcode, out actions)) {
                Log.Error($"消息没有处理: {opcode} {message}");
                return;
            }
            // 这里就不明白：它的那些 Domain 什么的
            SceneType sceneType = session.DomainScene().SceneType; // 【会话框】：哈哈哈，这是会话框两端，哪一端的场景呢？感觉像是会话框的什么Domain 场景？
            foreach (MessageDispatcherInfo ev in actions) {
                if (ev.SceneType != sceneType) {
                    continue;
                }
                try {
                    ev.IMHandler.Handle(session, message);
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
    }
}