using System;
namespace ET {
    public class CoroutineLock: IDisposable {
		// 【协程】：每次Scheduler 分配它了CPU 的执行时间，执行且仅只执行一个MoveNext() 完成的地方，所以它的【回收】——是设置协程下一段的调用内容、或说回调

        private int type;
        private long key;
        private int level; // 层级：细看一下这个参数
        public static CoroutineLock Create(int type, long k, int count) {
            CoroutineLock coroutineLock = ObjectPool.Instance.Fetch<CoroutineLock>();
            coroutineLock.type = type;
            coroutineLock.key = k;
            coroutineLock.level = count;
            return coroutineLock;
        }

        public void Dispose() {
			// 加入到【协程锁组件的、下一桢待执行的、队列】里：执行协程的下一个逻辑块 level+1
            CoroutineLockComponent.Instance.RunNextCoroutine(this.type, this.key, this.level + 1); 
            this.type = CoroutineLockType.None; // 回收当前的锁
            this.key = 0;
            this.level = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }
}