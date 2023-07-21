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
        private CoroutineLock currentCoroutineLock; // 当前锁：几乎可以用这个成员来标记，协程等待锁队列，是否为空？
// 【协程等待锁队列】：类里有两个类型标记变量，标记这个队列里，所有使用锁的上下文大致场景，和（请求锁）的 actorId （不对？不一定对？，某些情况下是【被请求锁】的 actorId）, 【场景 + actorId】对应一个队列（被请求锁 actorId 的例子：多个向location查询同一个实体真实进程号地址（在访问跨进程实体时），访问一次获得进程地址即可【这句网上抄的，但感觉是对的】）去理解的话，锁所想要锁住的是多个线程想要来读的共享资源，那么应该是被查询地址的小伙伴 me 的 actorId
        // 这个队列，用来管理：单个 actorId 实例请求过的，所有这同一类锁的上下文使用场景（一个特定场景）的所有请求过的锁，可以不止一把，所以用队列管理
        // 队列里的元素：只有超时回收时间上的区别（超时时长一致，但请求锁的请求时间点不同），是否已经抛异常回收或用完回收，与否的区别，其它完全一致，
        // 要用时，随意【！！不是随意好吧，队列是排队的，是按入队时间，就是比如请求小伙伴 me 的位置信息的所有请求者的请求时间？】取遍历到的第一把来用
        // 要用时，按【队列：先进先出】按请求共享资源的、先来后到的请求顺序，释放共享资源给用的。
        // 这里【理解重点是：】要想明白的是：这些锁，分门别类的协程异步等待锁，它们所调控、管理的逻辑本质是：【保障保证多进程间共享资源的安全、合理释放与使用】
        private readonly Queue<WaitCoroutineLock> queue = new Queue<WaitCoroutineLock>(); 
        public int Count {
            get {
                return this.queue.Count;
            }
        }
        // 【这个方法，读得狠费劲】：调用的地方请求要拿一把锁；如果队列为空，就创建一把并返回；如果先前有把同类型的在等，就创建新的加队列里，并返回当前新锁的索引给调用方
        // 为什么要【添加异步等待锁的等待时间变量】？来实现【两套机制】，或多了个清理垃圾的备用机制？
        // using() 调用锁使用的地方使用完毕自动回收；这是任何，非协程异步等待锁，都公用的机制
        // 【等待时间超时的第二套机制】：【为什么这套机制，也需要】？如同前面框架破烂开发者说由于 bug 或是进程挂掉、宕机，导致队列里的锁无法自动回收，会影响应用程序服务器性能，亲爱的表哥的活宝妹觉得，弱弱猫猫的开发者，整个这个第二超时机制，可能也是同样的原因！
        // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
        public async ETTask<CoroutineLock> Wait(int time) { // 【这个方法没看懂：】：协程锁在不同使用情境下需要等待的时长不一样。如特例活宝妹守着亲爱的表哥，是要是会守一辈子的
// 当前锁为空：是说队列为空吗？是的。只有在刚创建，或是回收之后，当前任务才可能会为空。队列为空，就是没有锁在等待，直接返回一把锁。等待时间完全忽略了。。 using(xxx)
// 针对这个key没有任何异步在处理，所以我们直接创建一个CoroutineLock 类，返回给使用的地方，让之前请求锁的地方可以直接往下运行（这句网上抄的，不知道抄对地方没有）
            if (this.currentCoroutineLock == null) { // 第一把锁：并没有加入队列中去 
                this.currentCoroutineLock = CoroutineLock.Create(type, key, 1); // level: 感觉，是纪录协程桢数序号，从 1 开始， 100-Warning
// 直接返回：返回一把锁，不管它时间怎么样的？。。。这个当前锁，被 using() 调用它的地方，持有索引引用，并在用完后负责回收释放等。负责释放，时间就没有关系了，自动忽略不计？！！！
                return this.currentCoroutineLock; 
            }
// 下面：当队列不为空，要创建新的等待锁，并仍然返回当前锁。为什么要创建新的等待锁？每个锁锁一个特异使用的地方，不同的锁可能等待的时间长短不一样、起止时间不一样等，先前没释放的还在用，需要创建新的，区分对待
            // 如果已经存在key对应的CoroutineLockQueue，则说明之前已经有异步针对这个key请求过至少一个锁（且还没有释放），创建一个ETTask<CoroutineLock>,通过CoroutineLockQueue的Add方法内部创建一个协程锁信息CoroutineLockInfo对象加入到对应的CoroutineLockQueue队列中，让请求的地方停止往下运行（即await后面的代码会等待异步完成，异步相关请看之前的文章）【这句也是网上抄的】
            WaitCoroutineLock waitCoroutineLock = WaitCoroutineLock.Create(); // 创建等待锁
            this.queue.Enqueue(waitCoroutineLock); // 新创建的锁，加入队列，有等待时间就设置一个闹钟。将值赋给当前锁。。。是可以添加元素的
            if (time > 0) { // 有等待时间：创建一个一次性闹钟。新锁的时间可以不用管，这里的时间为什么一定要管？这个时间标定：新创建等待锁的，最迟回收时间，时间到会自动回收。但是，它是可以早回收的，当 using() 代码块执行完的时候？！！好像是这样
                long tillTime = TimeHelper.ClientFrameTime() + time;
                // 闹钟时间到，会自动抛一个锁超时异常。超时时，应该是调用它的地方（using）代码块的自动回收。也就是锁超时时，锁回收。回收前，锁住的逻辑应该是执行完了的
                // 【协程锁超时，自动检测回调到TimerCoreInvokeType.CoroutineTimeout 标记的类： WaitCoroutineLockTimer 类】超时后会回收掉释放系统资源
                TimerComponent.Instance.NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock);
            } 
            // 下面，当赋值给当前锁currentCoroutineLock，先前的当前锁呢？既没有加入队列，又直接拿走了索引？为什么先前的当前锁，就直接释放了？？？
            this.currentCoroutineLock = await waitCoroutineLock.Wait(); // 这里，只是等待协程锁异步任务创建的完成，返回一把可用的锁，并不是等到最迟务必回收的时间。
            return this.currentCoroutineLock; // 返回：新创建的等待锁，设定了最迟回收时间的锁，但是这把锁是可以早于最迟回收时间回收的！！！
        }
        // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！爱表哥，爱生活！！！】
        public void Notify(int level) { // 哪里会调用这个方法？只更新了 level 桢数或桢序号？为的是协程分支往下一个分支执行
            // 有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock 【这个说明，看不懂】
// 遍历一遍队列【想要找到一把、非空可用锁】：
            // 当前队列是，入队时间升序，超时时间【应该也是升序】，因为同样的锁的使用上下文场景（对应锁的默认超时时长一致），只有入队时间的先后不同与区别
            while (this.queue.Count > 0) { // 遍历队列：取有效锁住的第一请求资源者，释放资源？这里还得再想想，感觉还没能想透，不是释放资源，不是说释放一把锁，帮助请求方移往下一个逻辑执行分支的吗？去找，组件管理类里，什么情况下，需要调用 Notify() 方法 
                WaitCoroutineLock waitCoroutineLock = queue.Dequeue();
                if (waitCoroutineLock.IsDisposed()) continue; // 已经超时异常或回收（从 using() 处的用完回收）。队列中还有索引，但是空引用。从队列中将其清除
                CoroutineLock coroutineLock = CoroutineLock.Create(type, key, level); // 只有参数 level: 这个变量是干什么的？
                waitCoroutineLock.SetResult(coroutineLock); // 释放了一把，队列里当前非空元素waitCoroutineLock，结果写回去了呀。调用方协程分支可以向前推进一步了呀
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
