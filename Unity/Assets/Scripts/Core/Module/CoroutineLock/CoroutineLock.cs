using System;
namespace ET {
    public class CoroutineLock: IDisposable { // 【协程锁】： level 是什么意思呢？
        private int type;
        private long key;
        private int level;
        public static CoroutineLock Create(int type, long k, int count) {
            CoroutineLock coroutineLock = ObjectPool.Instance.Fetch<CoroutineLock>(); // 对象池管理，回收再利用等
            coroutineLock.type = type;
            coroutineLock.key = k;
            coroutineLock.level = count;
            return coroutineLock;
        }
        public void Dispose() {
            // 【下一行】：都要回收了，不知道下一行在做什么？
            CoroutineLockComponent.Instance.RunNextCoroutine(this.type, this.key, this.level + 1); // <<<<<<<<<<<<<<<<<<<< 跟进去
            this.type = CoroutineLockType.None;
            this.key = 0;
            this.level = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }
}