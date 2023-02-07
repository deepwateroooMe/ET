using System.IO;

namespace ET.Server {

    // 知道对方的instanceId，使用这个类发actor消息: 不知道这里是谁注解的，搞清楚没有，构造函数，是发送者标识，还是接收者标识？
    public readonly struct ActorMessageSender {

        public long ActorId { get; } // 搞清楚没有，构造函数，是发送者标识，还是接收者标识？按道理上说，应该是发送者标识，那接收者的封在请求消息里吗＞
        // 最近接收或者发送消息的时间
        public long CreateTime { get; }
        
        public IActorRequest Request { get; }
        public bool NeedException { get; }
        public ETTask<IActorResponse> Tcs { get; }

        public ActorMessageSender(long actorId, IActorRequest iActorRequest, ETTask<IActorResponse> tcs, bool needException) {
            this.ActorId = actorId;
            this.Request = iActorRequest;
            this.CreateTime = TimeHelper.ServerNow();
            this.Tcs = tcs;
            this.NeedException = needException;
        }
    }
}