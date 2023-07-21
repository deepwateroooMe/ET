using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
namespace ET {
    // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！爱表哥，爱生活！！！】
    [AsyncMethodBuilder(typeof (ETAsyncTaskMethodBuilder))]
    public class ETTask: ICriticalNotifyCompletion {
        public static Action<Exception> ExceptionHandler;
        public static ETTaskCompleted CompletedTask {
            get {
                return new ETTaskCompleted();
            }
        }
        // Static: ETTask 类所有实例，所共有的。【类自带对象池】：这个静态队列，管理1000 个实例，无GC ETTask 
        // 【亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
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
            if (queue.Count > 1000) return;
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
        public bool IsCompleted { // 前段时间，看【异步状态机】时，记得哪里，是会每步检查异步任务是否结束的。找出来看一下
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
        public void GetResult() { // 什么地方，调用这个？这个我就是没能找到【我需要：去理解异步状态机中，什么地方什么情况下检查这个结果】才能够把这一块儿理解透彻
            switch (this.state) {
            case AwaiterStatus.Succeeded: // 状态结束：会回收
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
            if (this.state != AwaiterStatus.Pending) 
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            this.state = AwaiterStatus.Succeeded;
            Action c = this.callback as Action;
            this.callback = null;
            c?.Invoke();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public void SetException(Exception e) {
            if (this.state != AwaiterStatus.Pending) 
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
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
        public T GetResult() { // 谁，哪里调用想要拿这个结果？
            switch (this.state) {
            case AwaiterStatus.Succeeded: // 正常结果：
                T v = this.value; // 类型匹配
                this.Recycle(); // 回收异步任务的包装
                return v; // 返回去，给调用方 T 类型
            case AwaiterStatus.Faulted:
                ExceptionDispatchInfo c = this.callback as ExceptionDispatchInfo;
                this.callback = null;
                this.Recycle();
                c?.Throw(); // 【ETTask 异步结果异常：】真正抛出的地方
                return default; // 这里 default: 是指，下面的 default 分支，缺省异常
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
        // 【写结果】：先前不懂的地方是：ETTask 写好结果后，如果是IMHandler 的实体类，框架底层IMHandler 类的方法里，会把返回消息发回去 reply() 到客户端进程；
        // 【写结果】：可如果是当前【客户端】会话框上，异步结果ETTask 写好后，框架底层接下来的原理，还不懂。。
        public void SetResult(T result) { // 【这里】，前两天看过写结果的地方，但是没能找到拿结果 GetResult() 的调用的地方。隐藏在框架什么地方呢？
            if (this.state != AwaiterStatus.Pending) 
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            this.state = AwaiterStatus.Succeeded; // 这里，去看：当写成最终状态，是否有必要的回调。。
            this.value = result;
            Action c = this.callback as Action;
            this.callback = null;  // 标明：异步状态机的最后一步
            c?.Invoke();
        }
        [DebuggerHidden]
        // 这里写得相对诡异：要真正想明白了。【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要嫁给亲爱的表哥！！爱表哥，爱生活！！！】
        // 我上次读，以为设置好异常，ETTask 库是会自动抛出异常的；
        // 可是现在再看，它所谓的回调 callback，不该是异步状态机的下一个状态吗？什么时候ETTask 真的抛过异常了？
        public void SetException(Exception e) { 
            if (this.state != AwaiterStatus.Pending) 
                throw new InvalidOperationException("TaskT_TransitionToFinal_AlreadyCompleted");
            this.state = AwaiterStatus.Faulted; // 写：ETTask 的异步异常结果：出错了。。
            Action c = this.callback as Action; // 拿到上一步（异步状态机上一步）的回调（或是如下一行示例，所包装过的异常）
            this.callback = ExceptionDispatchInfo.Capture(e); // 包装了一下：当前节点，要抛出的异常（当前包装的异常，应该是会状态机的下一步抛出去。但是对于写成了异常的结果，下一步是在拿结果 GetResult() 里看见异常才抛异常）
// 去执行协程状态机的下一步，本质是异步状态机的下一步。但异步状态机的下一步（可以是回调Action, 也可以是如上一行前一步曾经包装过的异常，这里抛出）
            c?.Invoke();  // 执行异步状态机的下一步：回调，或是真正系统里抛出异常
        }
    }
}