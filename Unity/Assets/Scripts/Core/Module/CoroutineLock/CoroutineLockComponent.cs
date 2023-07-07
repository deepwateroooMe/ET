using System;
using System.Collections.Generic;
namespace ET {
    
    // 【协程锁组件】单例：Update() 更新回调实现
    public class CoroutineLockComponent: Singleton<CoroutineLockComponent>, ISingletonUpdate { // Update() 生命周期函数调用 
        private readonly Dictionary<int, CoroutineLockQueueType> dictionary = new();
        private readonly Queue<(int, long, int)> nextFrameRun = new Queue<(int, long, int)>(); // 下一桢待更新的

        public override void Dispose() {
            this.nextFrameRun.Clear();
        }
        public void Update() { // 更新：每桢更新，一个个处理
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
        public async ETTask<CoroutineLock> Wait(int coroutineLockType, long key, int time = 60000) {
            CoroutineLockQueueType coroutineLockQueueType;
            if (!this.dictionary.TryGetValue(coroutineLockType, out coroutineLockQueueType)) {
                coroutineLockQueueType = new CoroutineLockQueueType(coroutineLockType);
                this.dictionary.Add(coroutineLockType, coroutineLockQueueType);
            }
            return await coroutineLockQueueType.Wait(key, time);
        }
        private void Notify(int coroutineLockType, long key, int level) {
            CoroutineLockQueueType coroutineLockQueueType;
            if (!this.dictionary.TryGetValue(coroutineLockType, out coroutineLockQueueType)) return;
            coroutineLockQueueType.Notify(key, level); // 【自顶向下】地调用，每桢更新时，自顶向下地更新
        }
    }
}