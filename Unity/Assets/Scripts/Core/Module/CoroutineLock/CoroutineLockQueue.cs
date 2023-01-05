using System;
using System.Collections.Generic;

namespace ET {

    public class CoroutineLockQueue {
        private int type;
        private long key;
        
        public static CoroutineLockQueue Create(int type, long key) {
            CoroutineLockQueue coroutineLockQueue = ObjectPool.Instance.Fetch<CoroutineLockQueue>(); // 类class: 从系统类管理池中抓来的:或创建的新的,或回收利用的
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
            if (time > 0) {
                long tillTime = TimeHelper.ClientFrameTime() + time;
                TimerComponent.Instance.NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock);
            }
            this.currentCoroutineLock = await waitCoroutineLock.Wait();
            return this.currentCoroutineLock;
        }
        public void Notify(int level) { // level: 这里看来，仍然是没有什么实际意义，除了计数（入队列里的个数？）统计
            // 有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock
            while (this.queue.Count > 0) {
                WaitCoroutineLock waitCoroutineLock = queue.Dequeue();
                if (waitCoroutineLock.IsDisposed()) { // 已经超时回收了
                    continue;
                }
                CoroutineLock coroutineLock = CoroutineLock.Create(type, key, level); // level: 还是个数标记吗 ?  既引起嫌疑，又实在读不出是什么意思 ？
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