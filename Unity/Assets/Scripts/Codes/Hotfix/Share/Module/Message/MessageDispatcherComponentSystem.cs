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
        // 扫描框架里的标签系【MessageHandler(SceneType)】
        private static void Load(this MessageDispatcherComponent self) {
            self.Handlers.Clear();
            HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (MessageHandlerAttribute));
            foreach (Type type in types) {
                IMHandler iMHandler = Activator.CreateInstance(type) as IMHandler; // 框架启动、任何一端启动时：一次性、批量生成、各种不同类型的、消息处理器实例，备用
				// 也是说：一种类型【消息处理器】，是可能处理【同一进程下？多个不同场景】里的、【同一网络操作码类型】的消息的？是的！
                if (iMHandler == null) {
                    Log.Error($"message handle {type.Name} 需要继承 IMHandler");
                    continue;
                }
                object[] attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false); // 消息处理器
                foreach (object attr in attrs) { // <<<<<<<<<<<<<<<<<<<< 这里，同一【消息处理器】，可以处理多个不同场景的、同一网络操作码的消息
                    MessageHandlerAttribute messageHandlerAttribute = attr as MessageHandlerAttribute;
                    Type messageType = iMHandler.GetMessageType();
					// 概念：大型网络游戏框架，可以有多种【不同类型、各司其职】的消息处理器；每种类型的、消息处理器，处理各自、特异 opcode 操作码的消息
                    ushort opcode = NetServices.Instance.GetOpcode(messageType); 
                    if (opcode == 0) {
                        Log.Error($"消息opcode为0: {messageType.Name}");
                        continue;
                    } // 下面：下面是创建一个【消息派发器】包装体，注册备用 
                    MessageDispatcherInfo messageDispatcherInfo = new (messageHandlerAttribute.SceneType, iMHandler); // 对应：场景类型，与消息处理器
                    self.RegisterHandler(opcode, messageDispatcherInfo);
                }
            }
        }
        private static void RegisterHandler(this MessageDispatcherComponent self, ushort opcode, MessageDispatcherInfo handler) {
            if (!self.Handlers.ContainsKey(opcode))  // 同一【网络操作码类型】，一个链条管理，不同场景下的【消息派发器包装体】
                self.Handlers.Add(opcode, new List<MessageDispatcherInfo>());
            self.Handlers[opcode].Add(handler); // 加入管理体系来管理
        }
        public static void Handle(this MessageDispatcherComponent self, Session session, object message) {
            List<MessageDispatcherInfo> actions;
            ushort opcode = NetServices.Instance.GetOpcode(message.GetType()); // 操作码
            if (!self.Handlers.TryGetValue(opcode, out actions)) {
                Log.Error($"消息没有处理: {opcode} {message}"); // 框架里，没有这类型 opcode 消息的，处理器
                return;
            }
            // 它的那些 Domain 什么的？遍历去拿到【会话框、对应场景】下的消息处理器，要求对应场景下的【消息处理器】去处理消息
			// 【场景】：另一层【术业有专攻、各司其职的、职责封装】。场景拥有固定类型的职责。
            SceneType sceneType = session.DomainScene().SceneType; // 【会话框】：这是会话框两端，哪一端的场景呢？总之是收消息的那一端的场景？【TODO】：明天上午，把这个看下
            foreach (MessageDispatcherInfo ev in actions) {
                if (ev.SceneType != sceneType) 
                    continue;
                try { // 【会话框】界定的收消息的场景：场景下的处理器，处理消息
                    ev.IMHandler.Handle(session, message); // 处理分派消息：也就是调用IMHandler 接口的方法来处理消息
                } catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
    }
}