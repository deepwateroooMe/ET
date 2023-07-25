using System.Collections.Generic;
namespace ET {
    public class CoroutineLockQueueType { // 【协程锁队列】：区分类型。。。这几个类，相互之间的嵌套比较深比较多
        private readonly int type; // 以不同的类型，相区分：是前面定义过的几种使用锁的上下文场景类型
        // 字典：键，标明不同的类型，值是队列
        private readonly Dictionary<long, CoroutineLockQueue> coroutineLockQueues = new Dictionary<long, CoroutineLockQueue>();

        public CoroutineLockQueueType(int type) {
            this.type = type;
        }
        private CoroutineLockQueue Get(long key) {
            this.coroutineLockQueues.TryGetValue(key, out CoroutineLockQueue queue);
            return queue;
        }
        private CoroutineLockQueue New(long key) { // 从这里看，任何一个 key 时，都创建一个新队列。。。这里 key 不知道是什么，但如身份证般唯一标识，是字典的键
            CoroutineLockQueue queue = CoroutineLockQueue.Create(this.type, key); // 从这里来理解，Key 是什么？
            this.coroutineLockQueues.Add(key, queue); // 自己创建新的，也会自动加入到自己的管理体系中来
            return queue;
        }
        private void Remove(long key) {
            if (this.coroutineLockQueues.Remove(key, out CoroutineLockQueue queue)) 
                queue.Recycle();
        }
        public async ETTask<CoroutineLock> Wait(long key, int time) {
            CoroutineLockQueue queue = this.Get(key) ?? this.New(key); // 先取出队列，没有就创建一个新的队列
            return await queue.Wait(time);
        }
        public void Notify(long key, int level) { // 【管理组件 CoroutineLockComponent】，调用这个方法，才自顶向下的：【通知排队中的下一个：它可以持有共享资源，开始使用了】
            CoroutineLockQueue queue = this.Get(key);
            if (queue == null) return;
            // 1.如果对应的queue中没有其他人再请求了，则直接在CoroutineLockQueueType中删除这个key（即对应的CoroutineLockQueue释放了），这样后续又有请求这个key对应锁时，会发现对应的CoroutineLockQueue没有，可以直接获取锁了，
            if (queue.Count == 0) this.Remove(key); // 【共享资源，没人在排队，没人要用】
            // 2.如果队列中还有其他请求过【这个协程锁对应的key的协程锁】，则从队列中拿出对应的协程锁信息类CoroutineLockInfo，然后新建一个协程锁对象，并设置CoroutineLockInfo内部对应的ETTASK的Tcs.SetResult，让之前请求锁的异步继续执行。这样就是释放锁，让下一个等待相同key值的协程（或是using(){代码块}代码块，的逻辑）继续往下运行了。
            queue.Notify(level); 
        }
    }
}