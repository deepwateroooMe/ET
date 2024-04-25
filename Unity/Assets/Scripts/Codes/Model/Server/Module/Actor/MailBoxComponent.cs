namespace ET.Server {
    // 挂上这个组件表示该Entity是一个Actor,接收的消息将会队列处理
    [ComponentOf] // 它，可以有多个，不同的父级控件
    public class MailBoxComponent: Entity, IAwake, IAwake<MailboxType> {
        // Mailbox的类型
        public MailboxType MailboxType { get; set; }
    }
}