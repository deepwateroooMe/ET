using System.Collections.Generic;
namespace ET {
    public class MessageDispatcherInfo {
        public SceneType SceneType { get; }
        public IMHandler IMHandler { get; }
        public MessageDispatcherInfo(SceneType sceneType, IMHandler imHandler) {
            this.SceneType = sceneType;
            this.IMHandler = imHandler;
        }
    }
    // 消息分发组件:
    [ComponentOf(typeof(Scene))] // 场景上的组件
    public class MessageDispatcherComponent: Entity, IAwake, IDestroy, ILoad {
        public static MessageDispatcherComponent Instance {
            get;
            set;
        }
        public readonly Dictionary<ushort, List<MessageDispatcherInfo>> Handlers = new();
    }
}