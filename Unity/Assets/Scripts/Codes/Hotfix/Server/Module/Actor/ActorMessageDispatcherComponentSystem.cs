using System;
using System.Collections.Generic;
namespace ET.Server {

    [FriendOf(typeof(ActorMessageDispatcherComponent))] // Actor消息分发组件：对于管理器里的，对同一发送消息类型，不同场景下不同处理器的链表管理，多看几遍
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
        private static void Load(this ActorMessageDispatcherComponent self) { // 加载：程序域加载的时候
            self.ActorMessageHandlers.Clear(); // 清空
            var types = EventSystem.Instance.GetTypes(typeof (ActorMessageHandlerAttribute)); // 扫描程序域里的特定消息处理器标签 
            foreach (Type type in types) {
                object obj = Activator.CreateInstance(type); // 加载时：框架封装，自动创建【消息处理器】实例
                IMActorHandler imHandler = obj as IMActorHandler;
                if (imHandler == null) 
                    throw new Exception($"message handler not inherit IMActorHandler abstract class: {obj.GetType().FullName}");
                object[] attrs = type.GetCustomAttributes(typeof(ActorMessageHandlerAttribute), false);
                foreach (object attr in attrs) {
                    ActorMessageHandlerAttribute actorMessageHandlerAttribute = attr as ActorMessageHandlerAttribute;
                    Type messageType = imHandler.GetRequestType(); // 因为消息处理接口的封装：可以拿到发送类型
                    Type handleResponseType = imHandler.GetResponseType();// 因为消息处理接口的封装：可以拿到返回消息的类型
                    if (handleResponseType != null) {
                        Type responseType = OpcodeTypeComponent.Instance.GetResponseType(messageType);
                        if (handleResponseType != responseType) 
                            throw new Exception($"message handler response type error: {messageType.FullName}");
                    }
                    // 将必要的消息【发送类型】【返回类型】存起来，统一管理，备用
                    // 这里，对于同一发送消息类型, 是会、是可能存在【从不同的场景类型中返回，带不同的消息处理器】 以致于必须得链表管理
                    // 这里，感觉因为想不到、从概念上也地无法理解，可能会存在的适应情况、上下文场景，所以这里的链表管理同一发送消息类型，理解起来还有点儿困难
                    ActorMessageDispatcherInfo actorMessageDispatcherInfo = new(actorMessageHandlerAttribute.SceneType, imHandler);
                    self.RegisterHandler(messageType, actorMessageDispatcherInfo); // 存在本管理组件，所管理的字典里
                }
            }
        }
        private static void RegisterHandler(this ActorMessageDispatcherComponent self, Type type, ActorMessageDispatcherInfo handler) {
            // 这里，对于同一发送消息类型, 是会、是可能存在【从不同的场景类型中返回，带不同的消息处理器】 以致于必须得链表管理
            // 这里，感觉因为想不到、从概念上也地无法理解，可能会存在的适应情况、上下文场景，所以这里的链表管理同一发送消息类型，理解起来还有点儿困难
            if (!self.ActorMessageHandlers.ContainsKey(type)) 
                self.ActorMessageHandlers.Add(type, new List<ActorMessageDispatcherInfo>());
            self.ActorMessageHandlers[type].Add(handler);
        }
        public static async ETTask Handle(this ActorMessageDispatcherComponent self, Entity entity, int fromProcess, object message) {
            List<ActorMessageDispatcherInfo> list;  // 当一个场景创建，如何封装，加入管理体系的？【跟进这里去看】，把进程启动时的这此框架自动化，看明白
            if (!self.ActorMessageHandlers.TryGetValue(message.GetType(), out list)) // 根据消息的发送类型，来取所有可能的处理器包装链表 
                throw new Exception($"not found message handler: {message}");
            SceneType sceneType = entity.DomainScene().SceneType; // 定位：发送消息的实体，所在的场景类型
            foreach (ActorMessageDispatcherInfo actorMessageDispatcherInfo in list) { // 遍历：这个发送消息类型，所有存在注册过的消息处理器封装
                if (actorMessageDispatcherInfo.SceneType != sceneType)  // 场景不符就跳过
                    continue;
                // 定位：这个作用于进程上的【消息分发器组件】，是同一进程上所有场景的总管。当需要【发送消息】（过会儿看返回消息走这里吗？其它普通消息？应该走）时
                // 总管，仍是去找对应于【发送消息实体场景的特定场景（SceneType）】下的【IMActorHandler】实现实体，来处理相应逻辑。就是框架封装实现了相对条理化的管理
                actorMessageDispatcherInfo.IMActorHandler.Handle(entity, fromProcess, message); // 再住下跟：就需要找一个接口的实体实现类来看，改天再看。。   
            }
            await ETTask.CompletedTask;
        }
    }
}// 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】