using System.IO;
namespace ET.Server {
    // 知道对方的Id，使用这个类发actor消息
    [ChildOf(typeof(ActorLocationSenderComponent))]
    public class ActorLocationSender: Entity, IAwake, IDestroy {
        public long ActorId; // 【被查询位置消息】的小伙伴，的实例 id. 是会变化的，搬家了迁移进程了。重登录了。。。
        // 最近接收或者发送消息的时间
        public long LastSendOrRecvTime;
        public int Error; // 被查询小伙伴，位置查询消息发送专用代理，超时报错【被代理小伙伴搬家过程中。。。请稍候。。。】
    }
}