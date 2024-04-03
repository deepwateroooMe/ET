using System;
using System.Collections.Generic;
namespace ET {
    public class CoroutineLockQueue {

        private int type;  // 2 成员变量：标识 CoroutineLockQueue 的类型
        private long key;
        public static CoroutineLockQueue Create(int type, long key) {
            CoroutineLockQueue coroutineLockQueue = ObjectPool.Instance.Fetch<CoroutineLockQueue>();
            coroutineLockQueue.type = type;
            coroutineLockQueue.key = key;
            return coroutineLockQueue;
        }
        private CoroutineLock currentCoroutineLock; // 普通锁 
        private readonly Queue<WaitCoroutineLock> queue = new Queue<WaitCoroutineLock>(); // 等待锁队列：
        public int Count {
            get {
                return this.queue.Count;
            }
        }

		// 这个函数 Wait() 也是重点：
        public async ETTask<CoroutineLock> Wait(int time) {
            if (this.currentCoroutineLock == null) {
                this.currentCoroutineLock = CoroutineLock.Create(type, key, 1);
                return this.currentCoroutineLock; // 协程的第1 个逻辑段：是不需要等待、直接执行的、即刻返回
            }
            WaitCoroutineLock waitCoroutineLock = WaitCoroutineLock.Create();
            this.queue.Enqueue(waitCoroutineLock);
            if (time > 0) {
                long tillTime = TimeHelper.ClientFrameTime() + time;
				// 设置协程锁的1 次性闹钟：
                TimerComponent.Instance.NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock);
            }
// 用【使用协程锁的、原协程的眼光来看】：原协程的每一段，都是同一类型、不同的实例协程锁，所以这里实时更新【原协程每一协程段】的现、实例协程锁
            this.currentCoroutineLock = await waitCoroutineLock.Wait(); 
            return this.currentCoroutineLock;
        }
		// Notify 函数理解：可以把【协程锁】理解为，ET 框架里封装的、协程不同返回段的【实现帮助工具——借助协程锁CoroutineLock】，借助锁的这一桢一桢分段，来实现协程的必要的一桢一桢分段
		// 这里，协程等待锁，等待一桢，也就是要去，辅助必要的协程，说，你可以去执行你状态机的MoveNext() 逻辑段了
        public void Notify(int level) {
            // 有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock
            while (this.queue.Count > 0) {
                WaitCoroutineLock waitCoroutineLock = queue.Dequeue();
                if (waitCoroutineLock.IsDisposed()) { // 协程等待锁，超时回收了
                    continue;
                }
                CoroutineLock coroutineLock = CoroutineLock.Create(type, key, level); // 这些锁，都是工具。用这些小物件，标记：去执行相关必要的协程下一个逻辑段
                waitCoroutineLock.SetResult(coroutineLock); // 写结果：返回【原协程、可以！去执行下一协程段逻辑段的、协程锁】。这里协程锁，本身实例意义不大，除了类型标记外
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