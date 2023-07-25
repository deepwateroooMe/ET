using System;
using System.Threading;

namespace ET {
    
    // 【协程超时、自动检测机制】：问题是，协程原本就每桢执行一次，为什么有必要设置【 1 （毫）秒？？还是只是标注特殊类型】超时？
    [Invoke(TimerCoreInvokeType.CoroutineTimeout)] // 自动检测超时机制：不知道为什么定义静态类 TimerCoreInvokeType ？
    public class WaitCoroutineLockTimer: ATimer<WaitCoroutineLock> {
        protected override void Run(WaitCoroutineLock waitCoroutineLock) { // 设置回调：抛超时异常。为什么设计为抛超时异常？
            if (waitCoroutineLock.IsDisposed()) return; // 若已回收，再无它
            waitCoroutineLock.SetException(new Exception("coroutine is timeout!")); // 抛超时异常
        }
    }

    // 【异步资源异步等待锁】：这类锁的，本质是，自动化实现对共享资源的【挂号排队】。
    // 它帮助共享资源的调用方，站队排队, 它帮助自动化等待和实现，等到它所挂号的牌，可以开始持有和使用共享资源的时间。
    // 它内持一个ETTask 异步任务——帮助这个类实现异步自动化等待。异步等待的结果是，它等待到了它所排过的队、异步资源释放给它使用的时间，它返回调用方一把【共享资源独占锁】，给调用方使用共享资源。
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
        public void SetException(Exception exception) { // 主要是：【异步资源异步等待锁】等待超时，抛异常
            if (this.tcs == null) 
                throw new NullReferenceException("SetException tcs is null");
            var t = this.tcs;
            this.tcs = null; // 回收异步任务
            t.SetException(exception);
        }
        public bool IsDisposed() {
            return this.tcs == null;
        }
        public async ETTask<CoroutineLock> Wait() {
// 【等待异步任务的结果写好】：写好的结果，这里是一把拿到了索要资源的【保障共享资源安全】的、不站队排队的锁CoroutineLock
            // 每当回收一把刚使用完共享资源的锁（CoroutineLock-Dispose()），回收的过程与逻辑就是，通知队列中的下一个排队的锁：hi, 我用完了，你等到了你的顺序，你接下来就可以享用使用共享资源了！！
            // 异步等待，异步任务的结果（结果是：等到创建锁的索要共享资源的进程，拿到共享资源的时间点儿，等到这个点儿，等到属于它的顺序拿到了共享资源，创建锁的调用进程的 using(){} 块里的逻辑终于有资源可以开始执行了！！）写好呢？【现在，觉得更像是，后者！！】
            // 【爱表哥，爱生活！！！亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
            return await this.tcs;  // 等待异步任务的结果：本质是等到它排的队、挂的号轮到它的时间，返回一把【共享资源独占锁】
        }
    }
}