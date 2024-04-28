namespace ET {
	// 【会话框】：30 秒不活动，自动超时回收机制
    [ComponentOf(typeof(Session))]
    public class SessionIdleCheckerComponent: Entity, IAwake, IDestroy {
        public long RepeatedTimer;
    }
}