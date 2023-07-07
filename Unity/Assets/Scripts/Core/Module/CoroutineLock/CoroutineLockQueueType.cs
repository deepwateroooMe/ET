using System.Collections.Generic;
namespace ET {
    
    public class CoroutineLockQueueType { // 叫个什么，类型。。。
        private readonly int type;
        private readonly Dictionary<long, CoroutineLockQueue> coroutineLockQueues = new Dictionary<long, CoroutineLockQueue>();
        public CoroutineLockQueueType(int type) {
            this.type = type;
        }
        private CoroutineLockQueue Get(long key) {
            this.coroutineLockQueues.TryGetValue(key, out CoroutineLockQueue queue);
            return queue;
        }
        private CoroutineLockQueue New(long key) {
            CoroutineLockQueue queue = CoroutineLockQueue.Create(this.type, key);
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
        public void Notify(long key, int level) { // 再去找：上面是谁，调用这个方法，才自顶向下的？
            CoroutineLockQueue queue = this.Get(key);
            if (queue == null) return;
            if (queue.Count == 0) this.Remove(key);
            queue.Notify(level); // <<<<<<<<<<<<<<<<<<<< 【自顶向下】的调用
        }
    }
}
