using System;
using System.IO;
using System.Net;
namespace ET {
    // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！这一点儿，活宝妹永远清晰明确确定。爱表哥，爱生活！！！】
    // 【把这个注释看懂想透，感觉这个缓存问题就理解透彻了。爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！爱表哥，爱生活！！！】
    public abstract class AService: IDisposable { // 现在重点找：网络服务，读到消息后的大致过程
        public int Id { get; set; }
        public ServiceType ServiceType { get; protected set; }
        private (object Message, MemoryStream MemoryStream) lastMessageInfo;
        // 缓存上一个发送的消息，这样广播消息的时候省掉多次序列化。【在方法逻辑里，可以看懂】
        // 这样有个另外的问题,客户端就不能保存发送的消息来减少gc 【客户端GC 多的潜在问题。。。这一句，今天，暂时，难倒亲爱的表哥的弱弱活宝妹了。。。】改天会把它想明白的！！
        // 【上面一句，应该可以理解为，使用场景如客户端需要重复发某条消息时（如索要位置信息，但小伙伴搬家搬狠久发一次还要不到，客户端连发 10 次索要位置信息，每次消息内容完全相同）
        // 【上面：这里，自己的例子仍不确切，自己举了一个某种需求消息层面的重复消息，客户端是单线程还是多线程逻辑，同一个客户端同一时间点是否可能多条消息，消息发送队列在哪里？】
        // 【上面：但内存流上未必最后一条消息。需要是客户端发送消息队列里、或内存流层面上与最后一条消息相同】】，每次都得生成新的，这种场景下，客户端就会产生大量GC.
        // 【上面：自己想的是重复消息新实例的基本不可避免？的大量GC. 而标注应该是说后序的，重复消息的再次或是多次序列化至内存流，内存流上序列化发送层面的大量GC? 内存流是如何回收释放的？它不是重写覆盖先前内容不就可以了，甚至不用擦除？那么内存流是如何产生大量GC 的？这里自己刚才想错了，内存流是不会产生GC 的】
        // 【客户端需求：减少生成重复消息的消息层面新实例，来减少GC, 而非内存流层面的。因为内存流直接覆盖就行，没有GC. 缓存消息到内存流，这是现ET 设计的双端逻辑】
        // 【如果客户端能够缓存过这条消息，重发时，逻辑应该如何实现？同样【缓存内存流，内存流上发消息，省掉多次序列化的步骤】？感觉应该是这样。不对，客户端仍然缓存到内存流，跟现逻辑有什区别？完全没有区别，就是下面方法的逻辑】
        // 【客户端缓存消息内存流时，重复消息不需要先生成消息实例了吗？重复消息仍然需要生成新实例，仍然会产生大量GC。就是说，生成重复消息的新实例本身的GC 是不可避免的？不对。】
        // 不过这个问题并不大，因为客户端发送的消息是比较少的【真的吗？客户端不是要发狠多消息与各服交互的吗？不信】，
        // 如果实在需要，也可以修改这个方法，把outer的消息过滤掉。【框架开发者注的】可以看懂
        // 【客户端出去的都是 Outer 消息。这里【双端逻辑里】过滤掉 Outer 消息，就给了客户端【单端特异性】实现对Outer 消息客户端缓存逻辑的实现可能】
        // 【客户端单端Outer 消息缓存】：缓存客户端最后一条消息消息实例本身，而非内存流层面的。所以客户端的平衡就变成为，缓存重复消息的实例来减少GC, 但客户端的重复消息，就将不得不需要多次序列化的步骤。
        // 【客户端的问题：】又变成为，单线程还是多线程逻辑，如何客户端缓存最后一条消息？“最后”是在哪里确认为最后的？
        // 今天不钻这个牛角尖，改天再想。
        // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！这一点儿，活宝妹永远清晰明确确定。爱表哥，爱生活！！！】
        protected MemoryStream GetMemoryStream(object message) { // 拿消息的内存流：是（广播消息的时候）的一个使用需求
            if (object.ReferenceEquals(lastMessageInfo.Message, message)) { // 比较一下：就是上次发送的最后一条消息，直接返回其内存流
                Log.Debug($"message serialize cache: {message.GetType().FullName}");
                return lastMessageInfo.MemoryStream; // 广播消息，所有再次使用最后一条消息的使用情境，都省掉了多次序列化的步骤
            }
            (ushort _, MemoryStream stream) = MessageSerializeHelper.MessageToStream(message); // 新消息：需要序列化到内存流
            this.lastMessageInfo = (message, stream); // 缓存上一个发送的消息
            return stream;
        }

        public virtual void Dispose() {
        }
        public abstract void Update();
        public abstract void Remove(long id, int error = 0);
        public abstract bool IsDispose();
        public abstract void Create(long id, IPEndPoint address);
        // Send(): 找一个具体实现类，来看【发送消息】的逻辑
        public abstract void Send(long channelId, long actorId, object message);
        public virtual (uint, uint) GetChannelConn(long channelId) {
            throw new Exception($"default conn throw Exception! {channelId}");
        }
        public virtual void ChangeAddress(long channelId, IPEndPoint ipEndPoint) {
        }
    }
}
