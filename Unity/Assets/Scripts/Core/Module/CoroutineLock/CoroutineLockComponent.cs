using System;
using System.Collections.Generic;
namespace ET {

    // 【锁的两套机制：】创建时，可能的、有时间的等待Wait() 或是默认等1 分钟；时间到释放回收锁时的 Notify(). 把这两套都看懂
    // 【协程锁组件】单例类，不再是生成系：Update() 更新回调实现
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
// 【当前资源锁的回收】：【回收】当前资源的【共享资源当前独占锁】，也就是，通知队列中的下一个排队的锁，它可以使用共享资源了，所以叫做“Notify()”下一个等待者。。。
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
        // 【异步等待】： using() 调用的地方，只是异步等待【协程异步等待锁】的创建完成，返回锁的引用；并非等待锁的超时时间时长。。
        // 【回收释放】：两套机制。【感觉这里仍然还没能想明白想透彻】明天上午再把释放锁的机制，再好好搜索看一遍，或者今天晚上，如果有时间的话
        // using() 代码块一执行完，是可以早于默认的1 分钟后自动回收的；如果使用完毕，由于using的特性，会调用获取到的CoroutineLock的dispose （后半句，网上抄来的）
            // 由于【BUG：】或是进程挂掉，队列里的异步锁无法正常回收情况下的第二套自制，默认等待1 分钟后自动回收
        // Wait() 方法，自CoroutinelockComponent 单例组件，【自顶向下】，实现的都是，当前某把锁实例，等待方法【创建过程与分层管理】，也就是单例组件【自顶向下】创建一把异步锁的过程
        public async ETTask<CoroutineLock> Wait(int coroutineLockType, long key, int time = 60000) { // 所有几种不同的类型，都是等待1 分钟 
            CoroutineLockQueueType coroutineLockQueueType;
            if (!this.dictionary.TryGetValue(coroutineLockType, out coroutineLockQueueType)) {
                coroutineLockQueueType = new CoroutineLockQueueType(coroutineLockType);
                this.dictionary.Add(coroutineLockType, coroutineLockQueueType);
            }
            return await coroutineLockQueueType.Wait(key, time);
        }
        // 释放锁：部分还没能看懂。 level 是从哪里传来的值？这个变量，锁的层级，一再自增。释放回收锁，也是调用这里的方法， level 随着修练，功力自增！！
        private void Notify(int coroutineLockType, long key, int level) { // level ： 
            CoroutineLockQueueType coroutineLockQueueType;
            if (!this.dictionary.TryGetValue(coroutineLockType, out coroutineLockQueueType)) return;
            coroutineLockQueueType.Notify(key, level); // 【自顶向下】地调用，每桢更新时，自顶向下地更新
        }
    }
}