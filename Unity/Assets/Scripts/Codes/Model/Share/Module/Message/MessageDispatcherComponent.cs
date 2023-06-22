using System.Collections.Generic;
namespace ET {
    // 总管：对每个场景，消息分发器
    // 这个类，可以简单地理解为：先前的各种服，现在的各种服务端场景，它们所拥有的消息处理器实例的封装。那么默认，每种场景，只有一个消息处理器实体类
    public class MessageDispatcherInfo { 
        public SceneType SceneType { get; }
        public IMHandler IMHandler { get; }
        public MessageDispatcherInfo(SceneType sceneType, IMHandler imHandler) {
            this.SceneType = sceneType;
            this.IMHandler = imHandler;
        }
    }
    // 消息分发组件
    [ComponentOf(typeof(Scene))]
    public class MessageDispatcherComponent: Entity, IAwake, IDestroy, ILoad {
        public static MessageDispatcherComponent Instance { get; set; }
        public readonly Dictionary<ushort, List<MessageDispatcherInfo>> Handlers = new(); // 总管的字典
    }
}



