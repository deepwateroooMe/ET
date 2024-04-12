using System.Collections.Generic;
namespace ET.Server {
	// 这个组件的超时机制，细看过
    [ComponentOf(typeof(Scene))]
    public class ActorMessageSenderComponent: Entity, IAwake, IDestroy {
        public const long TIMEOUT_TIME = 40 * 1000; // 自动超时检测机制：Actor 消息发送器 40 秒超时
        public static ActorMessageSenderComponent Instance { get; set; }
        public int RpcId;
		// 组件管理字典：
        public readonly SortedDictionary<int, ActorMessageSender> requestCallback = new SortedDictionary<int, ActorMessageSender>();
        public long TimeoutCheckTimer;
        public List<int> TimeoutActorMessageSenders = new List<int>();
    }
}