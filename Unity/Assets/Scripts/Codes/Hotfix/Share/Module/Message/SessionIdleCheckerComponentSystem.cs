using System;
namespace ET {
    // 这是前面读过的、类似实现原理的超时机制
    [Invoke(TimerInvokeType.SessionIdleChecker)]
    public class SessionIdleChecker: ATimer<SessionIdleCheckerComponent> {
        protected override void Run(SessionIdleCheckerComponent self) {
            try {
                self.Check();
            } catch (Exception e) {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }
    [ObjectSystem]
    public class SessionIdleCheckerComponentAwakeSystem: AwakeSystem<SessionIdleCheckerComponent> {
        protected override void Awake(SessionIdleCheckerComponent self) {
            // 同样设置：【重复闹钟】：任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！！
            self.RepeatedTimer = TimerComponent.Instance.NewRepeatedTimer(SessionIdleCheckerComponentSystem.CheckInteral, TimerInvokeType.SessionIdleChecker, self);
        }
    }
    [ObjectSystem]
    public class SessionIdleCheckerComponentDestroySystem: DestroySystem<SessionIdleCheckerComponent> {
        protected override void Destroy(SessionIdleCheckerComponent self) { // 回收闹钟
            TimerComponent.Instance?.Remove(ref self.RepeatedTimer);
        }
    }
    public static class SessionIdleCheckerComponentSystem {
        public const int CheckInteral = 2000; // 每隔 2 秒
        public static void Check(this SessionIdleCheckerComponent self) {
            Session session = self.GetParent<Session>();
            long timeNow = TimeHelper.ClientNow();
            // 常量类定义：会话框最长每个 30 秒；
            // 判断：30 秒内，曾经发送过消息，并且也接收过消息，直接返回；否则，算作【会话框】超时
            if (timeNow - session.LastRecvTime < ConstValue.SessionTimeoutTime && timeNow - session.LastSendTime < ConstValue.SessionTimeoutTime) 
                return;
            Log.Info($"session timeout: {session.Id} {timeNow} {session.LastRecvTime} {session.LastSendTime} {timeNow - session.LastRecvTime} {timeNow - session.LastSendTime}");
            session.Error = ErrorCore.ERR_SessionSendOrRecvTimeout; // 【会话框】超时回收
            session.Dispose();
        }
    }
}