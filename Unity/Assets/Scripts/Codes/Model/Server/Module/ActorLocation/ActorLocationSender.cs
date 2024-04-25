using System.IO;
namespace ET.Server {
    // 知道对方的Id，使用这个类发actor消息【源】
	// 【TODO】：【ChildOf】属性，感觉凡是用到的地方，都被真正添加作为【子控件】建立起了【Component】级别的父子关系。
	// 这是亲爱的表哥的活宝妹，以前不曾注意，没能理解的。使用时，是一定得、建立起了【Component】级别的父子关系的吗？感觉这里不懂，需要学习网搜
    [ChildOf(typeof(ActorLocationSenderOneType))]
    public class ActorLocationSender: Entity, IAwake, IDestroy {
        public long ActorId;
        // 最近接收或者发送消息的时间
        public long LastSendOrRecvTime; // 记这个时间，也有什么超时回收机制吗？【TODO】：
        public int Error;
    }
}