using System.Collections.Generic;
namespace ET.Server {
	// 这个组件的超时机制，细看过
    [ComponentOf(typeof(Scene))]
    public class ActorMessageSenderComponent: Entity, IAwake, IDestroy {
        public const long TIMEOUT_TIME = 40 * 1000; // 自动超时检测机制：Actor 消息发送器 40 秒超时
        public static ActorMessageSenderComponent Instance { get; set; }
        public int RpcId;
		// 组件管理字典：排序字典，是因为有个超时自动检测；字典排序，超时检测时，就只扫字典的超时的那一部分就可以了
        public readonly SortedDictionary<int, ActorMessageSender> requestCallback = new SortedDictionary<int, ActorMessageSender>();
        public long TimeoutCheckTimer;
        public List<int> TimeoutActorMessageSenders = new List<int>();
    }
}