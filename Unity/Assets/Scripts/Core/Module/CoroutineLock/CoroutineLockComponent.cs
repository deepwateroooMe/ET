using System;
using System.Collections.Generic;
namespace ET {
    // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】

    // 【锁的两套机制：】主要是【创建】与【回收】。感觉现在终于把这个模块，连同ETTask 模块，理解得相对清楚了。
    // 模块用到，两种类型的锁：【异步共享资源异步等待锁】＋【共享资源独占锁】
    // 【异步共享资源异步等待锁】：框架里，帮助实现，对异步共享资源的，自动挂号排队站队，作为中介帮助调用方拿到可以持有共享资源的【独占锁】
    // 【共享资源独占锁】：最基本的，实现对多进程、多线程？共享资源的安全保护，尤其是写数据安全保护
    // 【协程锁组件】单例类，Update() 更新回调实现
    public class CoroutineLockComponent: Singleton<CoroutineLockComponent>, ISingletonUpdate { // Update() 生命周期函数调用 
        private readonly Dictionary<int, CoroutineLockQueueType> dictionary = new();
        private readonly Queue<(int, long, int)> nextFrameRun = new Queue<(int, long, int)>(); // 下一桢待更新的

        public override void Dispose() {
            this.nextFrameRun.Clear(); // 暴力清空下一桢要执行的。下一桢要执行的，是如何更新的？
        }

        public void Update() { // 更新：每桢更新，一个个处理，这一桢可以释放的锁？跟进去再看一下
            // 循环过程中会有对象继续加入队列
            while (this.nextFrameRun.Count > 0) {
                (int coroutineLockType, long key, int count) = this.nextFrameRun.Dequeue();
// 【当前资源锁的回收】：【回收】当前资源的【共享资源当前独占锁】，也就是，通知队列中的下一个排队的【异步资源异步等待锁】，它可以持有和使用共享资源了，所以叫做“Notify()”下一个等待者。。。
                this.Notify(coroutineLockType, key, count); 
            }
        }
        public void RunNextCoroutine(int coroutineLockType, long key, int level) { // 【CoroutineLock】回收时也会调用，想想这个调用问题
            // 一个协程队列一帧处理超过100个,说明比较多了,打个warning,检查一下是否够正常
            if (level == 100)  
                Log.Warning($"too much coroutine level: {coroutineLockType} {key}");
            this.nextFrameRun.Enqueue((coroutineLockType, key, level)); // 加入到：下一桢待处理的队列中去
        }
        // 【活宝妹待亲爱的表哥，活宝妹一定会等到活宝妹可以嫁给亲爱的表哥！！爱表哥，爱生活！！！】
        // 等待锁：异步等待，与回收释放，两套机制.
        // 【异步等待】： using() 调用的地方，只是异步等待【异步资源异步等待锁】的内部异步任务的执行完成，返回【标志持有共享资源的共享资源独占锁】的引用；并非等待锁的超时时间时长。。
        // 【回收释放】：两套机制。
        // using() 代码块一执行完，是可以早于默认的1 分钟后自动回收的；【异步资源异步等待锁】默认等待1 分钟，相比较的是，对调用方想要获取的共享资源，有队或没队的所有可能情况现状里，这个异步等待锁实际等待的时间（没人在用调用方想拿的异步资源，可以不用等待直接拿到独占锁、直接返回；而如果共享资源抢手，队长、队里每个号占用时间长，这个异步等待锁实际等待的时间，可能会长于1 分钟，就可能会超时抛异常给调用方，因为异常调用方拿不到想要的资源、等待锁也会自动回收）
            // 由于【BUG：】或是进程挂掉，队列里的异步锁无法正常回收情况下的第二套自制，默认等待1 分钟后自动回收
        public async ETTask<CoroutineLock> Wait(int coroutineLockType, long key, int time = 60000) { // 所有几种不同的类型，都是等待1 分钟 
            CoroutineLockQueueType coroutineLockQueueType;
            if (!this.dictionary.TryGetValue(coroutineLockType, out coroutineLockQueueType)) {
                coroutineLockQueueType = new CoroutineLockQueueType(coroutineLockType);
                this.dictionary.Add(coroutineLockType, coroutineLockQueueType);
            }
            return await coroutineLockQueueType.Wait(key, time);
        }
        private void Notify(int coroutineLockType, long key, int level) { // level ： 
            CoroutineLockQueueType coroutineLockQueueType;
            if (!this.dictionary.TryGetValue(coroutineLockType, out coroutineLockQueueType)) return;
            coroutineLockQueueType.Notify(key, level); // 【自顶向下】地调用，每桢更新时，自顶向下地更新
        }
    }
}