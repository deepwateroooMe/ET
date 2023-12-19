using System.Collections.Generic;
namespace ET.Server {

    [ComponentOf(typeof(Scene))]
    public class ActorMessageSenderComponent: Entity, IAwake, IDestroy {
        public const long TIMEOUT_TIME = 40 * 1000;
        public static ActorMessageSenderComponent Instance { get; set; }
        public int RpcId;
		// 从使用场景来看，下面，【有序字典，的，键】：是 response-rpcId
        public readonly SortedDictionary<int, ActorMessageSender> requestCallback = new SortedDictionary<int, ActorMessageSender>();
		
// 这个 long: 是重复闹钟的闹钟实例ID, 用来区分任何其它闹钟实例的
        public long TimeoutCheckTimer;
		// 【超时，接收者 rpcId 链条】：链条里存放的是，上面字典里的键，是【接收消息的 rpcId】
        public List<int> TimeoutActorMessageSenders = new List<int>(); // 这桢更新里：待发送给的（接收者rpcId）接收者链表
    }
}