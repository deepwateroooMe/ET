using System;
using System.Collections.Generic;
namespace ET { // 比较喜欢这一版设计封装更精致的源码：不至于每个文件好长
    public class CoroutineLockQueue {
        private int type;
        private long key;

        public static CoroutineLockQueue Create(int type, long key) {
            CoroutineLockQueue coroutineLockQueue = ObjectPool.Instance.Fetch<CoroutineLockQueue>();
            coroutineLockQueue.type = type;
            coroutineLockQueue.key = key;
            return coroutineLockQueue;
        }
        private CoroutineLock currentCoroutineLock;
        
        private readonly Queue<WaitCoroutineLock> queue = new Queue<WaitCoroutineLock>();
        public int Count {
            get {
                return this.queue.Count;
            }
        }
        public async ETTask<CoroutineLock> Wait(int time) {
            if (this.currentCoroutineLock == null) {
                this.currentCoroutineLock = CoroutineLock.Create(type, key, 1);
                return this.currentCoroutineLock;
            }
            WaitCoroutineLock waitCoroutineLock = WaitCoroutineLock.Create();
            this.queue.Enqueue(waitCoroutineLock);
    // 狠好玩：这里的逻辑就拆解为两大组件：时间闹钟管理，锁是另一模块。但时间到，可以回调到标签标注过的类的实例上来
            if (time > 0) {
                long tillTime = TimeHelper.ClientFrameTime() + time;
                // 下面，通过最后一个参数，将协程等待锁与闹钟组件，两个组件关联起来，回调的方式 
                TimerComponent.Instance.NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock); // <<<<<<<<<< waitCoroutineLock
            }
// 这里：感觉总是不知道它是在等什么？它等的逻辑是写在哪里的？应该是等待计时完成，闹钟的回调？那个标签太耍杂技了
            this.currentCoroutineLock = await waitCoroutineLock.Wait(); 
            return this.currentCoroutineLock;
        }
        public void Notify(int level) {
            // 有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock
            while (this.queue.Count > 0) {
                WaitCoroutineLock waitCoroutineLock = queue.Dequeue();
                if (waitCoroutineLock.IsDisposed()) {
                    continue;
                }
                CoroutineLock coroutineLock = CoroutineLock.Create(type, key, level);
                waitCoroutineLock.SetResult(coroutineLock);
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