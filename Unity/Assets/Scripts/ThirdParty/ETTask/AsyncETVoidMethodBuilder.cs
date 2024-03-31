using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
namespace ET {
	// 要看明白：这些内部封装的、构建器，状态机、什么的、之间的相互链接关系，看明白原理
    internal struct AsyncETVoidMethodBuilder {
        // 1. Static Create method.
        [DebuggerHidden]
        public static AsyncETVoidMethodBuilder Create() {
            AsyncETVoidMethodBuilder builder = new AsyncETVoidMethodBuilder();
            return builder;
        }
        // 2. TaskLike Task property(void)
        [DebuggerHidden]
        public ETVoid Task => default;
        // 3. SetException: 设置异常的地方，是真正调用过一次异常的
        [DebuggerHidden]
        public void SetException(Exception e) {
            ETTask.ExceptionHandler.Invoke(e); // 调用异常、执行异常
        }
        // 4. SetResult
        [DebuggerHidden]
        public void SetResult() {
            // do nothing
        }
        // 5. AwaitOnCompleted
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }
        // 6. AwaitUnsafeOnCompleted
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }
        // 7. Start
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine {
            stateMachine.MoveNext();
        }
        // 8. SetStateMachine
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine) {
        }
    }
}