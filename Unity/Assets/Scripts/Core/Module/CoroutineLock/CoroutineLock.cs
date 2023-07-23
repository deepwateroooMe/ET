using System;
namespace ET {
    
    // 【协程锁】：这个模块创建的目的，是为了有效管理、跨进程共享资源的“线程？”安全。所以，它以被保护共享资源的类型相区分
    // IDisposable: 对于任何IDisposable接口的类型，都可以使用using语句，而对于那些没有实现IDisposable接口的类型，使用using语句会导致一个编译错误。
    // IDisposable: 实现了这个接口，才可以使用 using() 代码块，使用完后，实现了 IDisposable 的协程锁才会调用 Dispose() 自动回收
    // 【using （）注意事项：】千万不要试图在using语句块外初始化对象。任何时候都应该在using语句中初始化需要使用的对象。
    // 【using （）注意事项：】using语句适用于清理单个非托管资源的情况，而多个非托管对象的清理最好以try-finnaly来实现，因为嵌套的using语句可能存在隐藏的Bug。内层using块引发异常时，将不能释放外层using块的对象资源；
    public class CoroutineLock: IDisposable { // 【协程锁】： level 是什么意思呢？
        private int type; // 共享资源：上下文使用场景、共享资源类型的区分
        private long key; // 共享资源：实倒 actorId （eg: 【被查询】位置信息的小伙伴的进程实例 actorId?）
        private int level; // 锁的层级，大概代表了共享资源的受欢迎抢手程度，有多长的队在排，在索要这份共享资源？
        public static CoroutineLock Create(int type, long k, int count) {
            CoroutineLock coroutineLock = ObjectPool.Instance.Fetch<CoroutineLock>(); // 对象池管理，回收再利用等
            coroutineLock.type = type;
            coroutineLock.key = k;
            coroutineLock.level = count;
            return coroutineLock;
        }
        public void Dispose() {
            // 【下一行】：回收是：检测一下层级是否超过100, 超过了可能是出错了打日志；否则回收：回收同样是加入到下一桢的执行队列，Update() 时才回收
            CoroutineLockComponent.Instance.RunNextCoroutine(this.type, this.key, this.level + 1); // <<<<<<<<<<<<<<<<<<<< 跟进去
            this.type = CoroutineLockType.None;
            this.key = 0;
            this.level = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }
}