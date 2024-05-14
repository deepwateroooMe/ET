using System.Collections.Generic;
namespace ET {
    public class Mailboxes {

        private readonly Dictionary<long, MailBoxComponent> mailboxes = new();
        public void Add(MailBoxComponent mailBoxComponent) {
			// 邮箱的父控件：也就是挂载了这个邮箱的 entity 的身份证实例号
            this.mailboxes.Add(mailBoxComponent.Parent.InstanceId, mailBoxComponent);
        }
        public void Remove(long instanceId) {
            this.mailboxes.Remove(instanceId);
        }
        public MailBoxComponent Get(long instanceId) {
            this.mailboxes.TryGetValue(instanceId, out MailBoxComponent entity);
            return entity;
        }
    }
}