using System;
using System.Collections.Generic;

namespace ET.Server {

    // Actor消息分发组件
    [FriendOf(typeof(ActorMessageDispatcherComponent))]
    public static class ActorMessageDispatcherComponentHelper {

// Awake Load Destroy
        [ObjectSystem]
        public class ActorMessageDispatcherComponentAwakeSystem: AwakeSystem<ActorMessageDispatcherComponent> {
            protected override void Awake(ActorMessageDispatcherComponent self) {
                ActorMessageDispatcherComponent.Instance = self;
                self.Awake();
            }
        }
        [ObjectSystem]
        public class ActorMessageDispatcherComponentLoadSystem: LoadSystem<ActorMessageDispatcherComponent> {
            protected override void Load(ActorMessageDispatcherComponent self) {
                self.Load();
            }
        }
        [ObjectSystem]
        public class ActorMessageDispatcherComponentDestroySystem: DestroySystem<ActorMessageDispatcherComponent> {
            protected override void Destroy(ActorMessageDispatcherComponent self) {
                self.ActorMessageHandlers.Clear(); // 它自己有个专门用来管理各种回调的字典,清空
                ActorMessageDispatcherComponent.Instance = null;
            }
        }
        private static void Awake(this ActorMessageDispatcherComponent self) {
            self.Load(); // 这时,它会加载 Load 两遍吗 ?
        }
        private static void Load(this ActorMessageDispatcherComponent self) {
            self.ActorMessageHandlers.Clear();
            var types = EventSystem.Instance.GetTypes(typeof (ActorMessageHandlerAttribute));
            foreach (Type type in types) {
                object obj = Activator.CreateInstance(type);
                IMActorHandler imHandler = obj as IMActorHandler;
                if (imHandler == null) {
                    throw new Exception($"message handler not inherit IMActorHandler abstract class: {obj.GetType().FullName}");
                }
                
                object[] attrs = type.GetCustomAttributes(typeof(ActorMessageHandlerAttribute), false);
                foreach (object attr in attrs) {
                    ActorMessageHandlerAttribute actorMessageHandlerAttribute = attr as ActorMessageHandlerAttribute;
                    Type messageType = imHandler.GetRequestType();
                    Type handleResponseType = imHandler.GetResponseType();
                    if (handleResponseType != null) {
                        Type responseType = OpcodeTypeComponent.Instance.GetResponseType(messageType);
                        if (handleResponseType != responseType) {
                            throw new Exception($"message handler response type error: {messageType.FullName}");
                        }
                    }
                    ActorMessageDispatcherInfo actorMessageDispatcherInfo = new(actorMessageHandlerAttribute.SceneType, imHandler);
                    self.RegisterHandler(messageType, actorMessageDispatcherInfo);
                }
            }
        }
        
        private static void RegisterHandler(this ActorMessageDispatcherComponent self, Type type, ActorMessageDispatcherInfo handler) {
            if (!self.ActorMessageHandlers.ContainsKey(type)) {
                self.ActorMessageHandlers.Add(type, new List<ActorMessageDispatcherInfo>()); // 加到字典值的链表里
            }
            self.ActorMessageHandlers[type].Add(handler);
        }

        public static async ETTask Handle(this ActorMessageDispatcherComponent self, Entity entity, int fromProcess, object message) {
            List<ActorMessageDispatcherInfo> list;
            if (!self.ActorMessageHandlers.TryGetValue(message.GetType(), out list)) {
                throw new Exception($"not found message handler: {message}");
            }
            SceneType sceneType = entity.DomainScene().SceneType;
            foreach (ActorMessageDispatcherInfo actorMessageDispatcherInfo in list) {
                if (actorMessageDispatcherInfo.SceneType != sceneType) {
                    continue;
                }
                await actorMessageDispatcherInfo.IMActorHandler.Handle(entity, fromProcess, message);   
            }
        }
    }
}