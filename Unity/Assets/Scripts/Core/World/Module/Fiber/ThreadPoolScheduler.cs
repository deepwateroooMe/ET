using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
namespace ET {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
    internal class ThreadPoolScheduler: IScheduler {
        private readonly List<Thread> threads;
        private readonly ConcurrentQueue<int> idQueue = new();
        private readonly FiberManager fiberManager;
        public ThreadPoolScheduler(FiberManager fiberManager) {
            this.fiberManager = fiberManager;
            int threadCount = Environment.ProcessorCount; // 根据机器的硬件配制来的：机器有多少个核，【线程池】里就最多开多少条线程
            this.threads = new List<Thread>(threadCount);
            for (int i = 0; i < threadCount; ++i) { // 硬件物理机，有多少核，【线程池】里，就开多少条可同步执行的线程
                Thread thread = new(this.Loop);
                this.threads.Add(thread);
                thread.Start();
            }
        }
        private void Loop() { // Update() LateUpdate()
            int count = 0;
            while (true) {
                if (count <= 0) {
                    Thread.Sleep(1);
                    // count最小为1
                    count = this.fiberManager.Count() / this.threads.Count + 1;
                }
                --count;
                if (this.fiberManager.IsDisposed()) {
                    return;
                }
                if (!this.idQueue.TryDequeue(out int id)) {
                    Thread.Sleep(1);
                    continue;
                }
                Fiber fiber = this.fiberManager.Get(id);
                if (fiber == null) {
                    continue;
                }
                if (fiber.IsDisposed) {
                    continue;
                }
                Fiber.Instance = fiber;
                SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext); // 切至【纤程实例】所需要的上下文里去执行
                fiber.Update();
                fiber.LateUpdate();
                SynchronizationContext.SetSynchronizationContext(null); // 重新置空
                Fiber.Instance = null;
                this.idQueue.Enqueue(id); // 弄完，也还是重新加回去；到下一桢，才再扫除、到下一桢遍历执行到它时，可能已经无效了的
            }
        }
        public void Dispose() {
            foreach (Thread thread in this.threads) {
                thread.Join();
            }
        }
        public void Add(int fiberId) {
            this.idQueue.Enqueue(fiberId);
        }
    }
}