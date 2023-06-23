using System.Collections.Generic;
namespace ET.Server {
    [ComponentOf(typeof(Scene))]
    public class ActorMessageSenderComponent: Entity, IAwake, IDestroy {
        public const long TIMEOUT_TIME = 40 * 1000;
        public static ActorMessageSenderComponent Instance { get; set; }
        public int RpcId;
        public readonly SortedDictionary<int, ActorMessageSender> requestCallback = new SortedDictionary<int, ActorMessageSender>();
// 这个 long: 是重复闹钟的闹钟实例ID, 用来区分任何其它闹钟的
        public long TimeoutCheckTimer; 
        public List<int> TimeoutActorMessageSenders = new List<int>(); // 这桢更新里：待发送给的（接收者rpcId）接收者链表
    }
}