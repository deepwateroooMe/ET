using System.Collections.Generic;
namespace ET.Server {
    [ComponentOf(typeof(Scene))]
    public class ActorMessageSenderComponent: Entity, IAwake, IDestroy {
        public const long TIMEOUT_TIME = 40 * 1000;
        public static ActorMessageSenderComponent Instance { get; set; }
        public int RpcId;
        public readonly SortedDictionary<int, ActorMessageSender> requestCallback = new SortedDictionary<int, ActorMessageSender>();
        public long TimeoutCheckTimer;
        // 去理解组件的处理逻辑：是每桢每个 Update() 调用吗，取出一个不超时的待发送消息，通过 AcotorMessageSender 将这个不超时的消息发出去。可是每桢如果只处理1 个，处理不完怎么办，自动超时作废吗？没读懂，再去看
        public List<int> TimeoutActorMessageSenders = new List<int>(); // 这桢更新里：待发送给的（接收者rpcId）接收者链表
    }
}