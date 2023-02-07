using System;

namespace ET.Server {

// Awake Awake1: 都执行Awake()函数, 但是实现的逻辑是不一样的   
    [ObjectSystem]
    public class MailBoxComponentAwakeSystem: AwakeSystem<MailBoxComponent> {
        protected override void Awake(MailBoxComponent self) {
            self.MailboxType = MailboxType.MessageDispatcher;
        }
    }

    [ObjectSystem]
    public class MailBoxComponentAwake1System: AwakeSystem<MailBoxComponent, MailboxType> {
        protected override void Awake(MailBoxComponent self, MailboxType mailboxType) {
            self.MailboxType = mailboxType;
        }
    }
}