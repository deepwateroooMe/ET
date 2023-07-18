using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
namespace ET {
    [AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder))]
    public class ETTask: ICriticalNotifyCompletion {
        public static Action<Exception> ExceptionHandler;
        public static ETTaskCompleted CompletedTask {
            get {
                return new ETTaskCompleted();
            }
        }
        private static readonly Queue<ETTask> queue = new Queue<ETTask>(); // 异步任务类：带一个异步任务的对象池队列
        // 请不要随便使用ETTask的对象池，除非你完全搞懂了ETTask!!!
        // 假如开启了池,await之后不能再操作ETTask，否则可能操作到再次从池中分配出来的ETTask，产生灾难性的后果
        // SetResult的时候请现将tcs置空，避免多次对同一个ETTask SetResult
        public static ETTask Create(bool fromPool = false) { // 【不是异步方法】：应该是可以秒创建完成的
            if (!fromPool) 
                return new ETTask();
            if (queue.Count == 0) 
                return new ETTask() {fromPool = true};    
            return queue.Dequeue();
        }
        private void Recycle() { 
            if (!this.fromPool) // 原则：只有从池里取出来的，才返回池
                return;
            this.state = AwaiterStatus.Pending; // 【没明白：】回收时还设置为 Pending, 什么时候写的当前结果？应该是在回收前
            this.callback = null;
            // 太多了
            if (queue.Count > 1000) 
                return;
            queue.Enqueue(this);
        }
        private bool fromPool;
        private AwaiterStatus state;
        private object callback; // Action or ExceptionDispatchInfo:private 是什么情况下可以设置回调呢？
        private ETTask() {  }

        [DebuggerHidden]
        private async ETVoid InnerCoroutine() {
            await this;
        }
        [DebuggerHidden]
        public void Coroutine() {
            InnerCoroutine().Coroutine();
        }
        [DebuggerHidden]
        public ETTask GetAwaiter() {
            return this;
        }
        public bool IsCompleted {
            [DebuggerHidden]
            get {
                return this.state != AwaiterStatus.Pending; // 只要不是 Pending 状态，就是异步任务执行结束
            }
        }
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action action) { // 有谁会调用这个方法吗？
            if (this.state != AwaiterStatus.Pending) { // 如果当前异步任务执行结束，就触发非空回调
                action?.Invoke();
                return;
            }
            this.callback = action; // 任务还没有结束，就纪录回调备用
        }
        [DebuggerHidden]
        public void OnCompleted(Action action) { // 有谁会调用这个方法吗？会传回调进来？
            this.UnsafeOnCompleted(action);
        }
        [DebuggerHidden]
        public void GetResult() {
            switch (this.state) {
                case AwaiterStatus.Succeeded:
                    this.Recycle();
                    break;
                case AwaiterStatus.Faulted:
                    ExceptionDispatchInfo c = this.callback as ExceptionDispatchInfo;
                    this.callback = null;
                    this.Recycle();
                    c?.Throw();
                    break;
                default:
                    throw new NotSupportedException("ETTask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }
        [DebuggerHidden]
        public void SetResult() {
            if (this.state != AwaiterStatus.Pending) {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }
            this.state = AwaiterStatus.Succeeded;
            Action c = this.callback as Action;
            this.callback = null;
            c?.Invoke();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetException(Exception e) {
            if (this.state != AwaiterStatus.Pending) {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }
            this.state = AwaiterStatus.Faulted;
            Action c = this.callback as Action;
            this.callback = ExceptionDispatchInfo.Capture(e);
            c?.Invoke();
        }
		// public static ETTask CompletedTask() => throw new NotImplementedException();// 这种，一定查自己不小心干的！！！
	}
    [AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder<>))]
    public class ETTask<T>: ICriticalNotifyCompletion {
        private static readonly Queue<ETTask<T>> queue = new Queue<ETTask<T>>();
        // 请不要随便使用ETTask的对象池，除非你完全搞懂了ETTask!!!
        // 假如开启了池,await之后不能再操作ETTask，否则可能操作到再次从池中分配出来的ETTask，产生灾难性的后果
        // SetResult的时候请现将tcs置空，避免多次对同一个ETTask SetResult
        public static ETTask<T> Create(bool fromPool = false) {
            if (!fromPool) 
                return new ETTask<T>();
            if (queue.Count == 0) 
                return new ETTask<T>() { fromPool = true };    
            return queue.Dequeue();
        }
        private void Recycle() {
            if (!this.fromPool) 
                return;
            this.callback = null;
            this.value = default;
            this.state = AwaiterStatus.Pending;
            // 太多了
            if (queue.Count > 1000) 
                return;
            queue.Enqueue(this);
        }
        private bool fromPool;
        private AwaiterStatus state;
        private T value;
        private object callback; // Action or ExceptionDispatchInfo
        private ETTask() {
        }
        [DebuggerHidden]
        private async ETVoid InnerCoroutine() {
            await this;
        }
        [DebuggerHidden]
        public void Coroutine() {
            InnerCoroutine().Coroutine();
        }
        [DebuggerHidden]
        public ETTask<T> GetAwaiter() {
            return this;
        }
        [DebuggerHidden]
        public T GetResult() {
            switch (this.state) {
            case AwaiterStatus.Succeeded:
                T v = this.value;
                this.Recycle();
                return v;
            case AwaiterStatus.Faulted:
                ExceptionDispatchInfo c = this.callback as ExceptionDispatchInfo;
                this.callback = null;
                this.Recycle();
                c?.Throw();
                return default;
            default:
                throw new NotSupportedException("ETask does not allow call GetResult directly when task not completed. Please use 'await'.");
            }
        }
        public bool IsCompleted {
            [DebuggerHidden]
            get {
                return state != AwaiterStatus.Pending;
            }
        } 
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action action) {
            if (this.state != AwaiterStatus.Pending) {
                action?.Invoke();
                return;
            }
            this.callback = action;
        }
        [DebuggerHidden]
        public void OnCompleted(Action action) {
            this.UnsafeOnCompleted(action);
        }
        [DebuggerHidden]
        public void SetResult(T result) {
            if (this.state != AwaiterStatus.Pending) {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }
            this.state = AwaiterStatus.Succeeded;
            this.value = result;
            Action c = this.callback as Action;
            this.callback = null;
            c?.Invoke();
        }
        [DebuggerHidden]
        public void SetException(Exception e) { // 我上次读，以为设置好异常，ETTask 库是会自动抛出异常的；
            if (this.state != AwaiterStatus.Pending) {
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }
            this.state = AwaiterStatus.Faulted;
            Action c = this.callback as Action;
            this.callback = ExceptionDispatchInfo.Capture(e); // 这里看，感觉它仍然只是包装了一下，哪里真正抛出的呢
            c?.Invoke(); // 这里，更多的是，去执行协程状态机的下一步，不是真正意义上的回调
        }
    }
}