using System;
using System.Threading;

namespace ET {

    [Invoke(TimerCoreInvokeType.CoroutineTimeout)]
    public class WaitCoroutineLockTimer: ATimer<WaitCoroutineLock> {
        protected override void Run(WaitCoroutineLock waitCoroutineLock) {
            if (waitCoroutineLock.IsDisposed()) {
                return;
            }
            waitCoroutineLock.SetException(new Exception("coroutine is timeout!"));
        }
    }

    // 包含一个ETTask<CoroutineLock> Tcs;一个表示超时时间的Time。其中TCS用于其他类获取协程锁时，用于异步等待的类Task对象。
    public class WaitCoroutineLock {

        public static WaitCoroutineLock Create() {
            WaitCoroutineLock waitCoroutineLock = new WaitCoroutineLock();
            waitCoroutineLock.tcs = ETTask<CoroutineLock>.Create(true);
            return waitCoroutineLock;
        }
        // 包含一个ETTask<CoroutineLock> Tcs;一个表示超时时间的Time。其中TCS用于其他类获取协程锁时，用于异步等待的类Task对象。
        private ETTask<CoroutineLock> tcs;

        public void SetResult(CoroutineLock coroutineLock) {
            if (this.tcs == null) {
                throw new NullReferenceException("SetResult tcs is null");
            }
            var t = this.tcs;
            this.tcs = null;
            t.SetResult(coroutineLock);
        }

        public void SetException(Exception exception) {
            if (this.tcs == null) {
                throw new NullReferenceException("SetException tcs is null");
            }
            var t = this.tcs;
            this.tcs = null;
            t.SetException(exception);
        }
// 也就是说,运行过程中,这个可能在其它地方,早就被因为超时而标记为  this.tcs = null;了
        public bool IsDisposed() {  
            return this.tcs == null;
        }

        public async ETTask<CoroutineLock> Wait() {
            return await this.tcs;
        }
    }
}