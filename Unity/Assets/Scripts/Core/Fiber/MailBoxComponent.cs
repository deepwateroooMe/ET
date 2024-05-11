namespace ET {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	
	// 亲爱的表哥的活宝妹，先前有个概念错误。如果一个场景一条纤程，那么应该是用线程模拟纤程，而不是进程模拟纤程？？【是这样的！】【主线程，线程池，每个纤程一个线程，3种调度方式】
    [EntitySystemOf(typeof(MailBoxComponent))]
    [FriendOf(typeof(MailBoxComponent))]
    public static partial class MailBoxComponentSystem { // 静态类：所以与组件写在了一个文件
        [EntitySystem]       
        private static void Awake(this MailBoxComponent self, MailBoxType mailBoxType) {
            Fiber fiber = self.Fiber();
            self.MailBoxType = mailBoxType;
            self.ParentInstanceId = self.Parent.InstanceId;
            fiber.Mailboxes.Add(self);
        }
        [EntitySystem]
        private static void Destroy(this MailBoxComponent self) {
            self.Fiber().Mailboxes.Remove(self.ParentInstanceId);
        }
        // 加到mailbox 【源】：邮箱每收到一条消息，都 Invoke 一次【对应类型的、邮箱处理器】来分发
		// 建立了3 种不同类型的邮件分发处理回调类：每条消息，必定有一个回调类会负责分发【网关服转发给客户端、有序、无序回调类等】。那三个处理回调类加在哪个程序域里？服务端或是双端、热更新域里
        public static void Add(this MailBoxComponent self, Address fromAddress, MessageObject messageObject) {
            // 根据mailboxType进行分发处理
            EventSystem.Instance.Invoke((long)self.MailBoxType, new MailBoxInvoker() {MailBoxComponent = self, MessageObject = messageObject, FromAddress = fromAddress});
        }
    }
    public struct MailBoxInvoker {
        public Address FromAddress;
        public MessageObject MessageObject;
        public MailBoxComponent MailBoxComponent;
    }

    // 挂上这个组件表示该Entity是一个Actor,接收的消息将会队列处理
    [ComponentOf]
    public class MailBoxComponent: Entity, IAwake<MailBoxType>, IDestroy {
        public long ParentInstanceId { get; set; }
        // Mailbox的类型
        public MailBoxType MailBoxType { get; set; }
    }
}