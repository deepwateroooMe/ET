using System;
using System.Collections.Concurrent;
using System.Threading;
namespace ET {
	// 自定义：多线程间、上下文同步的逻辑；所谓同步，本质也是Unity 里的每桢Update() 执行相关【投到主线程】的回调之类的
    public class ThreadSynchronizationContext : SynchronizationContext {
        // 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行【源】
		// 上面，在单线程多进程框架里，线程同步，是否意味着、多进程的上下文同步、多进程里各单线程【每个进程里仅只一个线程】的多进程间的、线程同步？

		// 多线程同步：同步队列，当然需要【多线程安全】
        private readonly ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>(); // 线程同步队列 
        private Action a;
        public void Update() { // 每桢执行
            while (true) {
                if (!this.queue.TryDequeue(out a)) { // 遍历，直到尾巴，直到、这桢的队列空了
                    return;
                }
                try {
                    a(); // 回调
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        public override void Post(SendOrPostCallback callback, object state) {
            this.Post(() => callback(state)); // 注册回调
        }
        public void Post(Action action) { // 缓存同步任务 
            this.queue.Enqueue(action);
        }
    }
}