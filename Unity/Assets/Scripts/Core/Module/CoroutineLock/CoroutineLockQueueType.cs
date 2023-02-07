using System.Collections.Generic;
namespace ET {

    public class CoroutineLockQueueType {
        private readonly int type; 

        // 内部封装一个Dictionary<long, CoroutineLockQueue> dictionary用于针对每一个key，一个协程锁队列，方便查找        
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
            this.coroutineLockQueues.Add(key, queue);
            return queue;
        }
        private void Remove(long key) {
            if (this.coroutineLockQueues.Remove(key, out CoroutineLockQueue queue)) {
                queue.Recycle();
            }
        }
        public async ETTask<CoroutineLock> Wait(long key, int time) {
            CoroutineLockQueue queue = this.Get(key) ?? this.New(key);
            return await queue.Wait(time);
        }
        public void Notify(long key, int level) {
            CoroutineLockQueue queue = this.Get(key);
            if (queue == null) {
                return;
            }
            
            if (queue.Count == 0) {
                this.Remove(key);
            }
            queue.Notify(level);
        }
    }
}