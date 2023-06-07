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
    // Actor消息分发组件
    [ComponentOf(typeof(Scene))]
    public class ActorMessageDispatcherComponent: Entity, IAwake, IDestroy, ILoad {
        [StaticField]
        public static ActorMessageDispatcherComponent Instance;
        // 下面的字典：去看下，同一类型，什么情况下会有一个链表的不同消息分发处理器？
        public readonly Dictionary<Type, List<ActorMessageDispatcherInfo>> ActorMessageHandlers = new();
    }
}