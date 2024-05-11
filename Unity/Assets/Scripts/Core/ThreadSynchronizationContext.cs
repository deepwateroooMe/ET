using System;
using System.Collections.Concurrent;
using System.Threading;
namespace ET {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	// 亲爱的表哥的活宝妹，以前没太细想，同进程多线程下，线程上下文同步问题，可是现在，脑袋里还有点儿糊。。
    public class ThreadSynchronizationContext : SynchronizationContext {
        // 线程同步队列,发送接收socket回调都放到该队列,由poll线程统一执行【源】：
		// poll 线程【TODO】：好像是个特指？亲爱的表哥的活宝妹，先前Unity 多线程同步用到一个库，有没有 poll?
        private readonly ConcurrentQueue<Action> queue = new();
        private Action a;
        public void Update() { // 每桢更新，这个是，在当前上下文——当前线程里每桢更新的
            while (true) {
                if (!this.queue.TryDequeue(out a)) {
                    return;
                }
                try {
                    a();
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        public override void Post(SendOrPostCallback callback, object state) {
            this.Post(() => callback(state));
        }
        public void Post(Action action) {
            this.queue.Enqueue(action);
        }
    }
}