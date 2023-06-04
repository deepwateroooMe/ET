using System.IO;
namespace ET.Server {
    // 知道对方的Id，使用这个类发actor消息
<<<<<<< HEAD
    [ChildOf(typeof(ActorLocationSenderComponent))]
    public class ActorLocationSender: Entity, IAwake, IDestroy {
=======
    [ChildOf(typeof(ActorLocationSenderOneType))]
    public class ActorLocationSender: Entity, IAwake, IDestroy
    {
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
        public long ActorId;
        // 最近接收或者发送消息的时间
        public long LastSendOrRecvTime;
        public int Error;
    }
}