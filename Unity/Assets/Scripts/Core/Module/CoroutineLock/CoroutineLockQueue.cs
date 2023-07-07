using System;
using System.Collections.Generic;
namespace ET {
    public class CoroutineLockQueue { // 【协程锁队列】
        private int type; // 类型
        private long key; // ？
        public static CoroutineLockQueue Create(int type, long key) {
            CoroutineLockQueue coroutineLockQueue = ObjectPool.Instance.Fetch<CoroutineLockQueue>();
            coroutineLockQueue.type = type;
            coroutineLockQueue.key = key;
            return coroutineLockQueue;
        }
        private CoroutineLock currentCoroutineLock; // 当前锁：不懂这个成员变量 
        private readonly Queue<WaitCoroutineLock> queue = new Queue<WaitCoroutineLock>(); // 【协程等待锁队列】
        public int Count {
            get {
                return this.queue.Count;
            }
        }
        public async ETTask<CoroutineLock> Wait(int time) { // 【这个方法】：协程锁在不同使用情境下需要等待的时长不一样。如特例活宝妹守着亲爱的表哥，是要是会守一辈子的
            // 从这里开始迷糊：两个不同的返回分支：
            if (this.currentCoroutineLock == null) {
                this.currentCoroutineLock = CoroutineLock.Create(type, key, 1); // level: 感觉，是纪录协程桢数序号，从 1 开始， 100-Warning
                return this.currentCoroutineLock; // 直接返回：返回一把锁，不管它时间怎么样的？。。。没懂
            }
            WaitCoroutineLock waitCoroutineLock = WaitCoroutineLock.Create(); // 创建等待锁
            this.queue.Enqueue(waitCoroutineLock); // 加入队列
            if (time > 0) { // 有等待时间：创建一个一次性闹钟。
                long tillTime = TimeHelper.ClientFrameTime() + time;
                // 重点去看：闹钟时间到，会做什么？【协程锁超时，自动检测回调到TimerCoreInvokeType.CoroutineTimeout 标记的类】超时后又只是回收掉。理解上逻辑连贯不起来
                // 等待的时间到了，等待协程锁异步任务会返回？？？
                TimerComponent.Instance.NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock);
            }
            this.currentCoroutineLock = await waitCoroutineLock.Wait();
            return this.currentCoroutineLock;
        }
        public void Notify(int level) { // 哪里会调用这个方法？只更新了 level 桢数或桢序号？
            // 有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock
            while (this.queue.Count > 0) { // 遍历一遍队列：队列是，入队时间升序，超时时间没有、不曾排序的
                WaitCoroutineLock waitCoroutineLock = queue.Dequeue();
                if (waitCoroutineLock.IsDisposed()) continue; // 已经超时回收。这里，相当于从队列中将其清除
                CoroutineLock coroutineLock = CoroutineLock.Create(type, key, level);
                waitCoroutineLock.SetResult(coroutineLock); // 这些，狠嵌套。也狠欠扁，看不懂。。。
                break;
            }
        }
        public void Recycle() {
            this.queue.Clear();
            this.key = 0;
            this.type = 0;
            this.currentCoroutineLock = null;
            ObjectPool.Instance.Recycle(this);
        }
    }
}