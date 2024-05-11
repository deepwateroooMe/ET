using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
namespace ET {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	// 【主线程调度】：应该算是【ET 框架多线程多进程、加纤程后的】最主要核心的、进程线程纤程、调度机制
	// 执行了2 个Unity 里的生命周期、每桢回调：Update() LateUpdate()
    internal class MainThreadScheduler: IScheduler {
        private readonly ConcurrentQueue<int> idQueue = new();
        private readonly ConcurrentQueue<int> addIds = new();
        private readonly FiberManager fiberManager;
        private readonly ThreadSynchronizationContext threadSynchronizationContext = new();
        public MainThreadScheduler(FiberManager fiberManager) {
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
            this.fiberManager = fiberManager;
        }
        public void Dispose() {
            this.addIds.Clear();
            this.idQueue.Clear();
        }
        public void Update() { // 【每桢 Update()】
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
            this.threadSynchronizationContext.Update();
            int count = this.idQueue.Count;
            while (count-- > 0) {
                if (!this.idQueue.TryDequeue(out int id)) { 
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
                SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);
                fiber.Update(); // 线程内的、每个有效纤程，Update() 一次
                Fiber.Instance = null;
                // 重新加回，以便下一桢继续执行；没有加多。上面那些过期或是回收的都是 continue, 所以没有加多，当前桢已经清除它们了
                this.idQueue.Enqueue(id); 
            }
            // Fiber调度完成，要还原成默认的上下文，否则unity的回调会找不到正确的上下文
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
        }
        public void LateUpdate() {
            int count = this.idQueue.Count;
            while (count-- > 0) {
                if (!this.idQueue.TryDequeue(out int id)) {
                    continue;
                }
                Fiber fiber = this.fiberManager.Get(id);
                if (fiber == null) {
                    continue;
                }
                if (fiber.IsDisposed) {
                    continue;
                }
                Fiber.Instance = fiber; // 当前的纤程实例
				// 下面：相当于是切到了，遍历的每个纤程实例、所在的它设置过的【线程】的上下文去执行了，就是凭用记，或程序员逻辑需求配置的
                SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);
                fiber.LateUpdate(); // 去执行了，当前纤程实例的 LateUpdate() 逻辑
                Fiber.Instance = null;
                
                this.idQueue.Enqueue(id);
            }
            while (this.addIds.Count > 0) { // 【TODO】：像是，新创建与添加的纤程，待执行，从下一桢开始执行
                this.addIds.TryDequeue(out int result);
                this.idQueue.Enqueue(result);
            }
            // Fiber调度完成，要还原成默认的上下文，否则unity的回调会找不到正确的上下文【源】
			// 上面，遍历执行各纤程实例时，各实例上下文，有可能与当前【主线程上下文】不一致【TODO】：不一致的情况，像没想透。所以遍历完后，再设置回来
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
        }
        public void Add(int fiberId = 0) {
            this.addIds.Enqueue(fiberId);
        }
    }
}