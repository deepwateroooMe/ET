namespace ET {

    // 【会话框】闲置状态管理组件：当服务器太忙，一个会话框闲置太久，有没有什么逻辑会回收闲置会话框来提高服务器性能什么之类的？
    [ComponentOf(typeof(Session))]
    public class SessionIdleCheckerComponent: Entity, IAwake, IDestroy {
        public long RepeatedTimer;
    }
}