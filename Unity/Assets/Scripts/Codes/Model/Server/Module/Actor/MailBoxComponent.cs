namespace ET.Server {
    // 挂上这个组件表示该Entity是一个Actor,接收的消息将会队列处理
    // 这个组件：不知道自己是否弄丢了什么重要的类，找不到什么【队列处理】，也还不明白这个模块的【Actor消息有死锁的可能，比如A call消息给B，B call给C，C call给A。因为MailboxComponent本质上是一个消息队列，它开启了一个协程会一个一个消息处理，返回ETTask表示这个消息处理类会阻塞MailboxComponent队列的其它消息。所以如果出现死锁，我们就不希望某个消息处理阻塞掉MailboxComponent其它消息的处理，我们可以在消息处理类里面新开一个协程来处理就行了】改天需要仔细来读，和理解这个模块的问题
    
    [ComponentOf]
    public class MailBoxComponent: Entity, IAwake, IAwake<MailboxType> {
        // Mailbox的类型
        public MailboxType MailboxType { get; set; }
    }
}