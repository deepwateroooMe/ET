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
    
    public class WaitCoroutineLock {

        public static WaitCoroutineLock Create() {
            WaitCoroutineLock waitCoroutineLock = new WaitCoroutineLock();
            waitCoroutineLock.tcs = ETTask<CoroutineLock>.Create(true);
            return waitCoroutineLock;
        }
        
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