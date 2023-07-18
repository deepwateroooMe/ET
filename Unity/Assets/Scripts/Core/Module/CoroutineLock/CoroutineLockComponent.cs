using System;
using System.Collections.Generic;
namespace ET {

    // 【锁的两套机制：】创建时，可能的、有时间的等待Wait() 或是默认等1 分钟；时间到释放回收锁时的 Notify(). 把这两套都看懂
    // 【协程锁组件】单例类，不再是生成系：Update() 更新回调实现
    public class CoroutineLockComponent: Singleton<CoroutineLockComponent>, ISingletonUpdate { // Update() 生命周期函数调用 
        private readonly Dictionary<int, CoroutineLockQueueType> dictionary = new();
        private readonly Queue<(int, long, int)> nextFrameRun = new Queue<(int, long, int)>(); // 下一桢待更新的

        public override void Dispose() {
            this.nextFrameRun.Clear();
        }

        public void Update() { // 更新：每桢更新，一个个处理，这一桢可以释放的锁？跟进去再看一下
            // 循环过程中会有对象继续加入队列
            while (this.nextFrameRun.Count > 0) {
                (int coroutineLockType, long key, int count) = this.nextFrameRun.Dequeue();
                this.Notify(coroutineLockType, key, count);
            }
        }
        public void RunNextCoroutine(int coroutineLockType, long key, int level) { // 【CoroutineLock】回收时也会调用，想想这个调用问题
            // 一个协程队列一帧处理超过100个,说明比较多了,打个warning,检查一下是否够正常
            if (level == 100) 
                Log.Warning($"too much coroutine level: {coroutineLockType} {key}");
            this.nextFrameRun.Enqueue((coroutineLockType, key, level)); // 加入到：下一桢待处理的队列中去
        }
        // 【今天下午：】这个不懂的模块再仔细读一遍。默认等待1 分钟。【活宝妹待亲爱的表哥，活宝妹一定会等到活宝妹可以嫁给亲爱的表哥！！爱表哥，爱生活！！！】
        // 等待锁：异步等待，与回收释放，两套机制，都要看懂
        public async ETTask<CoroutineLock> Wait(int coroutineLockType, long key, int time = 60000) { // 所有几种不同的类型，都是等待1 分钟 
            CoroutineLockQueueType coroutineLockQueueType;
            if (!this.dictionary.TryGetValue(coroutineLockType, out coroutineLockQueueType)) {
                coroutineLockQueueType = new CoroutineLockQueueType(coroutineLockType);
                this.dictionary.Add(coroutineLockType, coroutineLockQueueType);
            }
            return await coroutineLockQueueType.Wait(key, time);
        }
        // 释放锁：
        private void Notify(int coroutineLockType, long key, int level) { // 
            CoroutineLockQueueType coroutineLockQueueType;
            if (!this.dictionary.TryGetValue(coroutineLockType, out coroutineLockQueueType)) return;
            coroutineLockQueueType.Notify(key, level); // 【自顶向下】地调用，每桢更新时，自顶向下地更新
        }
    }
}