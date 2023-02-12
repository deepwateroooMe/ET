﻿using System;
using System.Collections.Generic;

namespace ET {

    // 消息分发组件
    [FriendOf(typeof(MessageDispatcherComponent))]
    public static class MessageDispatcherComponentHelper {

        // Awake Load Destroy
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

        // 这个方法会从所有组件中找到 标记有MessageHandlerAttribute的类遍历 => 反射创建该类的实例A  => 获取该Message的类型B,如下图的R2C_Ping
        // => 把B的编码作为key 实例A作为value 缓存到  ActorMessageDispatcherComponent 组件的ActorMessageHandlers字典中
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
                    
                    ushort opcode = NetServices.Instance.GetOpcode(messageType);
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
            List<MessageDispatcherInfo> actions; // 从自身字典里读出一个回调链表
            ushort opcode = NetServices.Instance.GetOpcode(message.GetType());
            if (!self.Handlers.TryGetValue(opcode, out actions)) {
                Log.Error($"消息没有处理: {opcode} {message}");
                return;
            }
            SceneType sceneType = session.DomainScene().SceneType; // <<<<<<<<<< 这里是特定的场景类型
            foreach (MessageDispatcherInfo ev in actions) {
                if (ev.SceneType != sceneType) {
                    continue;
                }
                try {
                    ev.IMHandler.Handle(session, message); // <<<<<<<<<< 符合要求的就触发回调
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
    }
}