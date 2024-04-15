using System;
namespace ET.Server {
    [ObjectSystem]
    public class MailBoxComponentAwakeSystem: AwakeSystem<MailBoxComponent> {
        protected override void Awake(MailBoxComponent self) {
            self.MailboxType = MailboxType.MessageDispatcher; // 缺省：都是这种类型的，消息派发器
        }
    }
    [ObjectSystem]
    public class MailBoxComponentAwake1System: AwakeSystem<MailBoxComponent, MailboxType> {
        protected override void Awake(MailBoxComponent self, MailboxType mailboxType) {
            self.MailboxType = mailboxType;
        }
    }
}


