using System;
using System.Collections.Generic;
namespace ET.Server {
    public class ActorMessageDispatcherInfo {
        public SceneType SceneType { get; }
        public IMActorHandler IMActorHandler { get; }
        public ActorMessageDispatcherInfo(SceneType sceneType, IMActorHandler imActorHandler) {
            this.SceneType = sceneType;
            this.IMActorHandler = imActorHandler;
        }
    }
    [ComponentOf(typeof(Scene))] // 场景的子组件 // Actor消息分发组件
    public class ActorMessageDispatcherComponent: Entity, IAwake, IDestroy, ILoad {
        [StaticField]
        public static ActorMessageDispatcherComponent Instance; // 全局单例吗？好像是，只在【服务端】添加了这个组件
        // 下面的字典：去看下，同一类型，什么情况下会有一个链表的不同消息分发处理器？ET7 重构后不是各种分区分小区管理吗？框架里，区的概念，会引起使用链表的必要吗？
        // 当多个分区可以放在一个进程，多个分区必然是不同场景类型吗？需要多想：这里使用链表 List 的原因
        public readonly Dictionary<Type, List<ActorMessageDispatcherInfo>> ActorMessageHandlers = new(); // <<<<<<<<<<<<<<<<<<<< List<>
    }
}