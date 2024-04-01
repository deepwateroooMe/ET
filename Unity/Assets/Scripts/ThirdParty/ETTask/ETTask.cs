using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
namespace ET {
	[AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder))]
    public class ETTask: ICriticalNotifyCompletion { // ETTask 类封装：自带非0 GC 缓存池，因为还是会产生部分垃圾回收
		// 【类的、静态成员变量】：为类的、所有实例所共享，使用同一套机制。内嵌【异步任务回收池】
        public static Action<Exception> ExceptionHandler;
        public static ETTaskCompleted CompletedTask {
            get {
                return new ETTaskCompleted(); // 永远，实例化新的
            }
        }
		// 类的静态成员变量：所有实例共享，任务回收池
        private static readonly Queue<ETTask> queue = new Queue<ETTask>(); // ETTask 类封装：自带非0 GC 缓存池，因为还是会产生部分垃圾回收
        // 请不要随便使用ETTask的对象池，除非你完全搞懂了ETTask!!! 【没能看懂：警告，是什么意思】
        // 假如开启了池,await之后不能再操作ETTask，否则可能操作到再次从池中分配出来的ETTask，产生灾难性的后果
        // SetResult的时候请现将tcs置空，避免多次对同一个ETTask SetResult
        public static ETTask Create(bool fromPool = false) {
            if (!fromPool) {
                return new ETTask();
            }
            if (queue.Count == 0) {
                return new ETTask() {fromPool = true};    
            }
            return queue.Dequeue();
        }

        private void Recycle() {
            if (!this.fromPool) {
                return;
            }
            this.state = AwaiterStatus.Pending;
            this.callback = null;
            // 太多了
            if (queue.Count > 1000) {
                return;
            }
            queue.Enqueue(this); // 缓存池的、自动回收再利用
        }
        private bool fromPool;
        private AwaiterStatus state;
        private object callback; // Action or ExceptionDispatchInfo 私有的话，是什么时候注册的？协程内部状态机 MoveNext 用作回调。。
        private ETTask() { }
		// 没能看懂：下面的2 个方法，包装来包装去，有什么方便的地方？
		[DebuggerHidden]
        private async ETVoid InnerCoroutine() {
            await this; // 【没看懂：就没明白，这是什么】
        }
        [DebuggerHidden]
        public void Coroutine() {
            InnerCoroutine().Coroutine(); // 是 ETVoid 类里、定义的公用空方法 ETVoid.Coroutine()
        }
		
        [DebuggerHidden]
        public ETTask GetAwaiter() {
            return this;
        }
        public bool IsCompleted {
            [DebuggerHidden]
            get {
                return this.state != AwaiterStatus.Pending;
            }
        }
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action action) { // 【注册、异步任务、完成的、回调函数】：去找哪里会调用这个，传这个回调进来？
            if (this.state != AwaiterStatus.Pending) {
                action?.Invoke(); // 这里，同一个回调，是有可能被执行、回调2 遍的？？ SetResult() 里去看、再看
                return;
            }
            this.callback = action; // 【赋值】：可是函数 UnsafeOnCompleted 是什么时候会被调用呢？状态机的下一步，算作回调。底层内部原理，仍然感觉不够懂
        }
        [DebuggerHidden]
        public void OnCompleted(Action action) { // 这里，传进来，继续反着找：就是感觉，还是不太理解，底层的贯穿实现逻辑，前后上下过程步骤、理解得不够透彻
            this.UnsafeOnCompleted(action);
        }
        [DebuggerHidden]
        public void GetResult() { // 【TODO】：去找，调用这个公用方法的地方
            switch (this.state) {
                case AwaiterStatus.Succeeded:
                    this.Recycle();
                    break;
				case AwaiterStatus.Faulted: // 真正的、抛出异常A
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
        public void SetResult() { // 【Pending ==> Succeeded 状态转变阶段】
            if (this.state != AwaiterStatus.Pending) { // 状态不合法、异常
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            }
            this.state = AwaiterStatus.Succeeded;
            Action c = this.callback as Action; // 去找：注册这个回调的地方【TODO】：
            this.callback = null;
            c?.Invoke(); // 执行注册过的非空回调：也是说，如果还不曾【执行回调】那么这里执行。一个回调仅只执行一次！
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
            c?.Invoke(); // 执行注册过的非空回调：也是说，如果还不曾【执行回调】那么这里执行。一个回调仅只执行一次！
        }
    }
	// 【泛型类】：定义
    [AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder<>))]
    public class ETTask<T>: ICriticalNotifyCompletion {
        private static readonly Queue<ETTask<T>> queue = new Queue<ETTask<T>>();
        // 请不要随便使用ETTask的对象池，除非你完全搞懂了ETTask!!!
        // 假如开启了池,await之后不能再操作ETTask，否则可能操作到再次从池中分配出来的ETTask，产生灾难性的后果
        // SetResult的时候请现将tcs置空，避免多次对同一个ETTask SetResult
        public static ETTask<T> Create(bool fromPool = false) {
            if (!fromPool) {
                return new ETTask<T>();
            }
            if (queue.Count == 0) {
                return new ETTask<T>() { fromPool = true };    
            }
            return queue.Dequeue();
        }
        private void Recycle() {
            if (!this.fromPool) {
                return;
            }
            this.callback = null;
            this.value = default;
            this.state = AwaiterStatus.Pending;
            // 太多了
            if (queue.Count > 1000) {
                return;
            }
            queue.Enqueue(this);
        }
        private bool fromPool;
        private AwaiterStatus state;
        private T value; // 泛型类：是带有返回结果的。这个用来，存结果
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
            this.value = result; // 结果
			// 回调：这里，应该是能够立即，通知到调用方，结果好了可用的！			
            Action c = this.callback as Action;
            this.callback = null;
            c?.Invoke(); // 注册过的回调的，合理调用。先前看回调是：状态机StateMachine.MoveNext() 作为回调。没看懂 
        }
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
    }
}