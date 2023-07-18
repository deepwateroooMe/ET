using System;
using System.Collections.Generic;
namespace ET {
    // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥，爱表哥，爱生活！！！】
    public class CoroutineLockQueue { // 【协程锁队列】: 队列里全是 WaitCoroutineLock
        private int type; // 类型：是框架里使用锁的几种不同的上下文场景类型
        private long key; // 类型里套【类型＋键】：不知道这个键 key，区分的是不同的需要发送消息等的ActorID 实例身份证号
        public static CoroutineLockQueue Create(int type, long key) {
            CoroutineLockQueue coroutineLockQueue = ObjectPool.Instance.Fetch<CoroutineLockQueue>();
            coroutineLockQueue.type = type;
            coroutineLockQueue.key = key;
            return coroutineLockQueue;
        }
        private CoroutineLock currentCoroutineLock; // 当前锁：不懂这个成员变量 
        private readonly Queue<WaitCoroutineLock> queue = new Queue<WaitCoroutineLock>(); // 【协程等待锁队列】  // <<<<<<<<<<<<<<<<<<<< readonly
        public int Count {
            get {
                return this.queue.Count;
            }
        }
        // 下面的方法：没看懂，网上找一下有没有什么解释？这个方法，改天需要再看一下。没看懂
        public async ETTask<CoroutineLock> Wait(int time) { // 【这个方法没看懂：】：协程锁在不同使用情境下需要等待的时长不一样。如特例活宝妹守着亲爱的表哥，是要是会守一辈子的
            if (this.currentCoroutineLock == null) { // 当前锁为空：是说队列为空吗？是的。只有在刚创建，或是回收之后，当前任务才可能会为空。上级管理持有当前队列索引 reference
                this.currentCoroutineLock = CoroutineLock.Create(type, key, 1); // level: 感觉，是纪录协程桢数序号，从 1 开始， 100-Warning
                return this.currentCoroutineLock; // 直接返回：返回一把锁，不管它时间怎么样的？。。。没懂。每个队列里的第一把锁，直接返回，不管时间，这里时间，是什么意思？
            } // 下面：当它不为空，要创建新的等待锁，并仍然返回当前锁。为什么要创建新的等待锁？
            WaitCoroutineLock waitCoroutineLock = WaitCoroutineLock.Create(); // 创建等待锁
            this.queue.Enqueue(waitCoroutineLock); // 新创建的锁，加入队列，有等待时间就设置一个闹钟。将值赋给当前锁。。。是可以添加元素的
            if (time > 0) { // 有等待时间：创建一个一次性闹钟。
                long tillTime = TimeHelper.ClientFrameTime() + time;
                // 闹钟时间到，会自动抛一个锁超时异常。超时时，应该是调用它的地方（using）代码块的自动回收。也就是锁超时时，锁回收。回收前，锁住的逻辑应该是执行完了的
                // 【协程锁超时，自动检测回调到TimerCoreInvokeType.CoroutineTimeout 标记的类： WaitCoroutineLockTimer 类】超时后会回收掉释放系统资源
                TimerComponent.Instance.NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock);
            }
            this.currentCoroutineLock = await waitCoroutineLock.Wait(); // 这里，只是等待协程锁异步任务创始的完成，并不是锁超时
            return this.currentCoroutineLock; // 返回：新创建的等待锁，是合理的
        }
        // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！爱表哥，爱生活！！！】
        public void Notify(int level) { // 哪里会调用这个方法？只更新了 level 桢数或桢序号？得网上再找下说明，看不懂
            // 有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock
            while (this.queue.Count > 0) { // 遍历一遍队列：队列是，入队时间升序，超时时间没有、不曾排序的
                WaitCoroutineLock waitCoroutineLock = queue.Dequeue();
                if (waitCoroutineLock.IsDisposed()) continue; // 已经超时回收。这里，相当于从队列中将其清除
                CoroutineLock coroutineLock = CoroutineLock.Create(type, key, level); // 只有参数 level: 这个变量是干什么的？
                waitCoroutineLock.SetResult(coroutineLock); // 这些，狠嵌套。也狠欠扁，看不懂。。。
                break;
            }
        }
        public void Recycle() {
            this.queue.Clear(); // 清空队列：是释放回收了队列里所有可能的元素。它是上级管理者创建的，这里会回收吗？会，就是上级字典的回收调用这里
            this.key = 0;
            this.type = 0;
            this.currentCoroutineLock = null; // 只有在当前任务被回收之后，它才可能为空
            ObjectPool.Instance.Recycle(this); // 释放回收这个类：到对象池，实现无GC. 它是一个队列，什么情况下会调用回收整个类、队列？
        }
    }
}



