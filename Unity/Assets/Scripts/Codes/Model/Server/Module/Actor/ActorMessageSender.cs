using System.IO;
namespace ET.Server {
    // 知道对方的instanceId，使用这个类发actor消息
    public readonly struct ActorMessageSender {
        public long ActorId { get; }
        // 最近接收或者发送消息的时间
        public long CreateTime { get; }
        public IActorRequest Request { get; }
        public bool NeedException { get; }
        public ETTask<IActorResponse> Tcs { get; } // 所有的：位置相关的ActorLocationMessage, 都是需要回复消息的；有不需要回复的IActorMessage 实例
        public ActorMessageSender(long actorId, IActorRequest iActorRequest, ETTask<IActorResponse> tcs, bool needException) {
            this.ActorId = actorId;
            this.Request = iActorRequest;
            this.CreateTime = TimeHelper.ServerNow();
            this.Tcs = tcs;
            this.NeedException = needException;
        }
    }
}