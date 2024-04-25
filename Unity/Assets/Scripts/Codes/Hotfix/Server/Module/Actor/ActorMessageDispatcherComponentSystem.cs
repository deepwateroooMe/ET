using System;
using System.Collections.Generic;
namespace ET.Server {
    // Actor消息分发组件
    [FriendOf(typeof(ActorMessageDispatcherComponent))]
    public static class ActorMessageDispatcherComponentHelper {
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
                self.ActorMessageHandlers.Clear();
                ActorMessageDispatcherComponent.Instance = null;
            }
        }
        private static void Awake(this ActorMessageDispatcherComponent self) {
            self.Load();
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
				// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
                object[] attrs = type.GetCustomAttributes(typeof(ActorMessageHandlerAttribute), false);
                foreach (object attr in attrs) {
                    ActorMessageHandlerAttribute actorMessageHandlerAttribute = attr as ActorMessageHandlerAttribute;
                    Type messageType = imHandler.GetRequestType();
					// 对【外网消息】：如某服下发客户端的IActorMessage 消息，返回类型是如何处理的？OuterMessage 是【内网服想要发给客户端，就直接发给网关服】，是永远不会走到这里来的！
					// 对于任何需要回复消息的 IActorRequest 消息，在其【发送逻辑】的调用过程里，应该会自己带有、标明有返回类型
					// 任何的OuterMessage-IActorMessage 都永远只是：【内网服务器】发给【网关服】的，不会、不需要任何的IActorMessage消息处理器，交由网关服直接下发客户端
                    Type handleResponseType = imHandler.GetResponseType();
                    if (handleResponseType != null) {
                        Type responseType = OpcodeTypeComponent.Instance.GetResponseType(messageType);
                        if (handleResponseType != responseType)
                        {
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
                self.ActorMessageHandlers.Add(type, new List<ActorMessageDispatcherInfo>());
            }
            self.ActorMessageHandlers[type].Add(handler);
        }
        public static async ETTask Handle(this ActorMessageDispatcherComponent self, Entity entity, int fromProcess, object message) {
            List<ActorMessageDispatcherInfo> list; // 一台物理机、某个进程上的、总管：对本进程内N 多场景统管【链条】
			// 【链条】：【TODO 需要确认，可能没理解透彻】：仍把同一进程上、同类型场景、不止一个节点的、多个同类型场景，理解为【分身分服】
			// 当某种类型服务器，因功能或游戏特性受压时，随时备份分身、1 个场景、分开多个N 个场景，来应对服务器处理压力，所以需要【链条】，同进程同类型场景，也不止一个！
            if (!self.ActorMessageHandlers.TryGetValue(message.GetType(), out list)) {
                throw new Exception($"not found message handler: {message} {entity.GetType().FullName}");
            }
            SceneType sceneType = entity.Domain.SceneType; // 【收件人 Entity】所属的、目标场景
            foreach (ActorMessageDispatcherInfo actorMessageDispatcherInfo in list) {
                if (!actorMessageDispatcherInfo.SceneType.HasSameFlag(sceneType)) { // 确保，仅只使用、目标场景上的、 actorMessage 消息派发器
                    continue;
                }
                await actorMessageDispatcherInfo.IMActorHandler.Handle(entity, fromProcess, message);   
            }
        }
    }
}