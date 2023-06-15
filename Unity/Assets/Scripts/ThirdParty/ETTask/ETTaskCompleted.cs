using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace ET {
    // 【自已的理解】：这里只是协程状态机的一个特殊的最终状态，类狠轻量，创建一个空任务，只等写结果（如果有必要的结果需要写的话，就是创建以备要用）
    [AsyncMethodBuilder(typeof (AsyncETTaskCompletedMethodBuilder))]
    public struct ETTaskCompleted: ICriticalNotifyCompletion {
        [DebuggerHidden]
        public ETTaskCompleted GetAwaiter() {
            return this;
        }
        [DebuggerHidden]
        public bool IsCompleted => true;
        [DebuggerHidden]
        public void GetResult() {
        }
        [DebuggerHidden]
        public void OnCompleted(Action continuation) {
        }
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action continuation) {
        }
    }
}