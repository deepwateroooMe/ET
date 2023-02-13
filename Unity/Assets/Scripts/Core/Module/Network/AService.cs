using System;
using System.IO;
using System.Net;

namespace ET {

    // 抽象基类：提供一些必要的方法
    public abstract class AService: IDisposable {

        public int Id { get; set; }
        public ServiceType ServiceType { get; protected set; }

        private (object Message, MemoryStream MemoryStream) lastMessageInfo;
        
        // 缓存上一个发送的消息，这样广播消息的时候省掉多次序列化,这样有个另外的问题,客户端就不能保存发送的消息来减少gc，
        // 不过这个问题并不大，因为客户端发送的消息是比较少的，如果实在需要，也可以修改这个方法，把outer的消息过滤掉。

        // [不懂]: 发消息与收消息，所使用的是同一个缓冲区吗？不是底层读写缓存区是分开的吗？
        // 上面,缓冲区里的消息数据是没有反序列化的，所以收到消息读取消息头分析处理，若是需要广播，就可以直接缓存区发广播，不必再序列化一次
        // 这个处理方式，客户端： 不能保存消息，客户端发送消息会需要多次创建，产生狠多GC. 但是作者认为客户端发送的消息比较少，GC 不会太多没问题
        // 若是修改，也就是说=内网消息，可以直接缓存冲发消息;其它消息，按客户端的需要，可以保存，不采用缓存区发消息？是这个意思吗？
        protected MemoryStream GetMemoryStream(object message) {
            if (object.ReferenceEquals(lastMessageInfo.Message, message)) { // 判定：是否是刚才收到的最后一条消息？是，就可以拿来直接用了
                Log.Debug($"message serialize cache: {message.GetType().Name}");
                return lastMessageInfo.MemoryStream;
            }
            (ushort _, MemoryStream stream) = MessageSerializeHelper.MessageToStream(message);
            this.lastMessageInfo = (message, stream);
            return stream;
        }
        
        public virtual void Dispose() { }
        public abstract void Update();
        public abstract void Remove(long id, int error = 0);
        
        public abstract bool IsDispose();
        public abstract void Create(long id, IPEndPoint address);
        public abstract void Send(long channelId, long actorId, object message);

        public virtual (uint, uint) GetChannelConn(long channelId) {
            throw new Exception($"default conn throw Exception! {channelId}");
        }
        public virtual void ChangeAddress(long channelId, IPEndPoint ipEndPoint) {
        }
    }
}