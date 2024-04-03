using System;
using System.Threading;
namespace ET {

	[Invoke(TimerCoreInvokeType.CoroutineTimeout)]
    public class WaitCoroutineLockTimer: ATimer<WaitCoroutineLock> {
        protected override void Run(WaitCoroutineLock waitCoroutineLock) { // 计时器时间到时：回调逻辑
            if (waitCoroutineLock.IsDisposed()) {
                return;
            }
            waitCoroutineLock.SetException(new Exception("coroutine is timeout!")); // 协程超时
        }
    }
    public class WaitCoroutineLock {
        public static WaitCoroutineLock Create() { // 静态方法：
            WaitCoroutineLock waitCoroutineLock = new WaitCoroutineLock();
            waitCoroutineLock.tcs = ETTask<CoroutineLock>.Create(true);
            return waitCoroutineLock;
        }
        private ETTask<CoroutineLock> tcs; 
// 就是上一级的Queue 里Notify 写结果时，会实例一把新的【同类型、协程等待锁】通知原协程、下一桢可以去执行协程的下一个协程段逻辑段
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
        public bool IsDisposed() {
            return this.tcs == null;
        }
        public async ETTask<CoroutineLock> Wait() {
// 等待这把【非空等待锁】：这个锁，想要有值、非空值，就务发等到原协程先前的所有协程段都执行完、原协程的上一个协程段执行完、并返回一把这个段的允许执行协程锁；此时这个锁、才会非空
            return await this.tcs; 
        }
    }
}