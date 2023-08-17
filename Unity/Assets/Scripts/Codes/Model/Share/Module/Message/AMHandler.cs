using System;
namespace ET {

    public abstract class AMHandler<Message>: IMHandler where Message : class {
        
        protected abstract ETTask Run(Session session, Message message); // ET7 原本的
        
        public void Handle(Session session, object msg) { // 因为这个方法是同步方法，所以不同内部调用 await, 换个写法就可以了
            Message message = msg as Message;
            if (message == null) {
                Log.Error($"消息类型转换错误: {msg.GetType().Name} to {typeof (Message).Name}");
                return;
            }
            if (session.IsDisposed) {
                Log.Error($"session disconnect {msg}");
                return;
            }
            this.Run(session, message).Coroutine(); // 同步方法：调用的是异步方法的协程？可以这么写吗》？
        }
        public Type GetMessageType() {
            return typeof (Message);
        }
        public Type GetResponseType() {
            return null;
        }
    }
}