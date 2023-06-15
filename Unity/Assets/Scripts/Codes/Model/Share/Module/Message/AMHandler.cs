using System;
namespace ET {
    public abstract class AMHandler<Message>: IMHandler where Message : class {

        protected abstract ETTask Run(Session session, Message message); // ET7 原本的
// 虽然我这么改，可以暂时消掉编译错误。但改得不对，现在消掉了编译错误，等编译通过，运行时错误会一再崩出来的。。。
        // protected abstract void Run(Session session, Message message); 

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