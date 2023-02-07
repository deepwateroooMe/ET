using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace ET {

    [AsyncMethodBuilder(typeof (AsyncETVoidMethodBuilder))]
    internal struct ETVoid: ICriticalNotifyCompletion {
        [DebuggerHidden]
        public void Coroutine() { // 这里是提供这个接口，然后供晚点儿状态机包装什么的时候再赋值吗？
        }
        [DebuggerHidden]
        public bool IsCompleted => true;
        [DebuggerHidden]
        public void OnCompleted(Action continuation) {
        }
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action continuation) {
        }
    }
}