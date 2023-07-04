using System;
using System.Threading;
namespace ET {
    // 【协程超时、自动检测机制】：问题是，协程原本就每桢执行一次，为什么有必要设置【 1 （毫）秒？？还是只是标注特殊类型】超时？
    [Invoke(TimerCoreInvokeType.CoroutineTimeout)] // 自动检测超时机制：不知道为什么定义静态类 TimerCoreInvokeType ？
    public class WaitCoroutineLockTimer: ATimer<WaitCoroutineLock> {
        protected override void Run(WaitCoroutineLock waitCoroutineLock) {
            if (waitCoroutineLock.IsDisposed()) return; // 若已回收，再无它
            waitCoroutineLock.SetException(new Exception("coroutine is timeout!")); // 抛超时异常
        }
    }
    public class WaitCoroutineLock {
        private ETTask<CoroutineLock> tcs; // 成员变量 
        public static WaitCoroutineLock Create() {
            WaitCoroutineLock waitCoroutineLock = new WaitCoroutineLock();
            waitCoroutineLock.tcs = ETTask<CoroutineLock>.Create(true); // <<<<<<<<<<<<<<<<<<<< 对象池中抓壮丁
            return waitCoroutineLock;
        }
        public void SetResult(CoroutineLock coroutineLock) {
            if (this.tcs == null) 
                throw new NullReferenceException("SetResult tcs is null");
            var t = this.tcs;
            this.tcs = null;
            t.SetResult(coroutineLock);
        }
        public void SetException(Exception exception) {
            if (this.tcs == null) 
                throw new NullReferenceException("SetException tcs is null");
            var t = this.tcs;
            this.tcs = null;
            t.SetException(exception);
        }
        public bool IsDisposed() {
            return this.tcs == null;
        }
        public async ETTask<CoroutineLock> Wait() {
            return await this.tcs;
        }
    }
}