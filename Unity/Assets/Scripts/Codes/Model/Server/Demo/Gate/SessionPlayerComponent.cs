namespace ET.Server {

// 功能唯一:  就是在网络Session过程中,帮助提供用户信息PlayerID的,信息简单减少带宽,应该是减少了网络申请往返的次数,或不必再去某个地方读用户标识     
    [ComponentOf(typeof(Session))]
    public class SessionPlayerComponent : Entity, IAwake, IDestroy {

        public long PlayerId { get; set; }
    }
}