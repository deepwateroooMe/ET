using System.IO;
namespace ET.Server {
    // 知道对方的instanceId，使用这个类发actor消息: 【原】，说是对方的，接收者的 instanceId
    public readonly struct ActorMessageSender {
        public long ActorId { get; } // 接收者的？
        // 最近接收或者发送消息的时间
        public long CreateTime { get; }
        public IActorRequest Request { get; }
        public bool NeedException { get; } // 标记：消息发送者，是否需要，过程中可能会出现的异常？不确信，就找个框架里实例看下
        public ETTask<IActorResponse> Tcs { get; }
        public ActorMessageSender(long actorId, IActorRequest iActorRequest, ETTask<IActorResponse> tcs, bool needException) {
            this.ActorId = actorId;
            this.Request = iActorRequest;
            this.CreateTime = TimeHelper.ServerNow(); // 自动封装：服务器端时间，方便框架底层自动封装【超时检测、与抛超时异常】
            this.Tcs = tcs;
            this.NeedException = needException;
        }
    }
}