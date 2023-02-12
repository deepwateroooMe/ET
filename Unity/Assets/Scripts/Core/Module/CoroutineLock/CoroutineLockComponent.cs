using System;
using System.Collections.Generic;
namespace ET {

    // 协程锁： 就像是多线程操作为保证线程安全，使用线程锁一样.在协程里，因为异步编程总会引起逻辑上一些先后关系给破坏掉了。为了保证逻辑上先后关系 引入协程锁。就跟线程的lock一样
    // 增加协程锁组件，locationComponent跟actor都使用协程锁来实现队列机制，代码大大简化，并且非常好懂。让消息可以队列处理而已
    // 协程锁原理很简单，同一个key只有一个协程能执行，其它同一个key的协程将队列，这个协程执行完会唤醒下一个协程。
    //     协程锁是个非常方便的组件，比如服务端在处理登录或者下线过程中，每个异步操作都可能同一个账号会再次登录上来，
    //     逻辑十分复杂，我们会希望登录某部分异步操作是原子操作，账号再次登录要等这个原子操作完成才能执行，
    //     这样登录或者下线过程逻辑复杂度将会简化十倍以上。    
    public class CoroutineLockComponent: Singleton<CoroutineLockComponent>, ISingletonUpdate {

        // 主要封装一个List<CoroutineLockQueueType> list按照CoroutineLockType类型，对应存放每一个CoroutineLockQueueType。
        // 内部还增加了用于协程超时的各种超时相关的数据结构：MultiMap<long, CoroutineLockTimer> timers，Queue<long> timeOutIds，Queue<CoroutineLockTimer> timerOutTimer
        private readonly List<CoroutineLockQueueType> list = new List<CoroutineLockQueueType>(CoroutineLockType.Max);
        private readonly Queue<(int, long, int)> nextFrameRun = new Queue<(int, long, int)>();

        public CoroutineLockComponent() {
// 100个:  是链表好，还是字典好  ？遍历: 两个数据结构，主要是链表并不到100个的时候，感觉会狠浪费空间 
            for (int i = 0; i < CoroutineLockType.Max; ++i) {
                CoroutineLockQueueType coroutineLockQueueType = new CoroutineLockQueueType(i);
                this.list.Add(coroutineLockQueueType);
            }
        }
        public override void Dispose() {
            this.list.Clear();
            this.nextFrameRun.Clear();
        }
        public void Update() {
            // 循环过程中会有对象继续加入队列
            while (this.nextFrameRun.Count > 0) {
                (int coroutineLockType, long key, int count) = this.nextFrameRun.Dequeue();
                this.Notify(coroutineLockType, key, count);
            }
        }
        public void RunNextCoroutine(int coroutineLockType, long key, int level) {
            // 一个协程队列一帧处理超过100个,说明比较多了,打个warning,检查一下是否够正常
            if (level == 100) {
                Log.Warning($"too much coroutine level: {coroutineLockType} {key}");
            }
            // 这里的level,更像是队列的元素个数计数,无关其它 锁引用次数,或资源回收相关
            this.nextFrameRun.Enqueue((coroutineLockType, key, level));
        }
        public async ETTask<CoroutineLock> Wait(int coroutineLockType, long key, int time = 60000) { // 默认1 分钟
            CoroutineLockQueueType coroutineLockQueueType = this.list[coroutineLockType];
            return await coroutineLockQueueType.Wait(key, time);
        }
        private void Notify(int coroutineLockType, long key, int level) {
            CoroutineLockQueueType coroutineLockQueueType = this.list[coroutineLockType];  // 链表的遍历:  相比于字典  ？  100  个
            coroutineLockQueueType.Notify(key, level);
        }
    }
}