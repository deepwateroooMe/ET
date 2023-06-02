using System;
namespace ET {
    public abstract class AMHandler<Message>: IMHandler where Message : class {
        // protected abstract ETTask Run(Session session, Message message);
        protected abstract void Run(Session session, Message message); // 这里不知道，会不会被我改坏掉
        
        public void Handle(Session session, object msg) {
            Message message = msg as Message;
            if (message == null) {
                Log.Error($"消息类型转换错误: {msg.GetType().Name} to {typeof (Message).Name}");
                return;
            }
            if (session.IsDisposed) {
                Log.Error($"session disconnect {msg}");
                return;
            }
            // this.Run(session, message).Coroutine();
            this.Run(session, message);
        }
        public Type GetMessageType() {
            return typeof (Message);
        }
        public Type GetResponseType() {
            return null;
        }
    }
}
