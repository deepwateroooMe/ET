using System;
namespace ET {

// 协程锁，获得这个类即代表获得了针对某个key的对象使用权，可以继续执行。
    public class CoroutineLock: IDisposable {
        private int type;
        private long key;
        private int level; // 原作者，应该标记一下，这个到底是肿么意思？
        
        public static CoroutineLock Create(int type, long k, int count) {
            CoroutineLock coroutineLock = ObjectPool.Instance.Fetch<CoroutineLock>();
            coroutineLock.type = type;
            coroutineLock.key = k;
            coroutineLock.level = count; // 
            return coroutineLock;
        }
        
        public void Dispose() {
            // level+1: 不知道它这么标记是什么意思, 索引次数的指针计数 ???
            CoroutineLockComponent.Instance.RunNextCoroutine(this.type, this.key, this.level + 1); // level: 这个比较好玩,它说协程的每一步每个节点步都是不同实例来管理
            
            this.type = CoroutineLockType.None;
            this.key = 0;
            this.level = 0;
            
            ObjectPool.Instance.Recycle(this); // 还是说,这把锁,必须得等到指针计数回0时才能真正的回收 ?
        }
    }
}