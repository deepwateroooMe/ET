using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
namespace ET {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	
    // 一个Fiber一个固定的线程【源】：
	// 那就不是【一个线程，可能存在多个 Fiber 呀】。【每个纤程占1 个线程】好处：方便管理，极度简化【1 个线程内多纤程、与多线程且每个线程内多纤程】的调度复杂性。就把纤程当线程用，区分主与非主线程
    internal class ThreadScheduler: IScheduler {
        private readonly ConcurrentDictionary<int, Thread> dictionary = new();
        private readonly FiberManager fiberManager;
        public ThreadScheduler(FiberManager fiberManager) {
            this.fiberManager = fiberManager;
        }
        private void Loop(int fiberId) {
            Fiber fiber = fiberManager.Get(fiberId);
            Fiber.Instance = fiber;
            SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);
            while (true) {
                if (this.fiberManager.IsDisposed()) {
                    return;
                }
                fiber = fiberManager.Get(fiberId);
                if (fiber == null) {
                    this.dictionary.Remove(fiberId, out _);
                    return;
                }
                if (fiber.IsDisposed) {
                    this.dictionary.Remove(fiberId, out _);
                    return;
                }
				// 调用2 回调
                fiber.Update();
                fiber.LateUpdate();
                Thread.Sleep(1); // 这是一个近似模拟，应该不算严格的每桢回调更新
            }
        }
        public void Dispose() {
            foreach (var kv in this.dictionary.ToArray()) {
                kv.Value.Join();
            }
        }
        public void Add(int fiberId) {
            Thread thread = new(() => this.Loop(fiberId)); // 【创建纤程】：本质是创建线程，把线程当纤程用。框架自己管理纤程的调度
            this.dictionary.TryAdd(fiberId, thread);
            thread.Start();
        }
    }
}