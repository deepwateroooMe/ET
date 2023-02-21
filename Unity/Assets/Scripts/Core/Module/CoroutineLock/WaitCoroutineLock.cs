using System;
using System.Threading;
namespace ET {
    [Invoke(TimerCoreInvokeType.CoroutineTimeout)] // 这里，像是设计了一个更高级一点儿，到时自动触发 Invoke 的超时机制，这个标注应该是与超时时间相关的
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
            waitCoroutineLock.tcs = ETTask<CoroutineLock>.Create(true); // 对象池抓一个
            return waitCoroutineLock;
        }
        private ETTask<CoroutineLock> tcs; // 唯一的异步任务成员
        // 个人理解：这里理解起来总是有点儿困难。这个，协程等待锁，异步的
        // 它只有一个异步任务，正常情况下，创建过程中，或是超时等待过程中，它的 tcs ！＝ null. 只有在它等待任务完成了，超时了解锁了，刚超时写结果来才会置空
        // 所以当，置空这个异步协程等待锁时，如果已经为空，可以抛异常
        public void SetResult(CoroutineLock coroutineLock) {
            if (this.tcs == null) {
                throw new NullReferenceException("SetResult tcs is null");
            }
            var t = this.tcs;
            this.tcs = null; // 其实到这一步，这个，协程等待锁，应该就可以回收再利用了
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
            return await this.tcs; // 这里还是有点儿想不明白：无法跟闹钟组件的超时关联起来，如果只是创建一把协程等待锁，缓存池里去取，不是很快吗？
        }
    }
}