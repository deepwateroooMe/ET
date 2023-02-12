using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace ET {

    [AsyncMethodBuilder(typeof (AsyncETVoidMethodBuilder))]
    internal struct ETVoid: ICriticalNotifyCompletion {

        [DebuggerHidden]
        // 要实现异步等待，那么实现的原理就是封装成协程，以实现异步等待协程的嵌套？
        // 这里是提供这个接口，然后供晚点儿状态机包装什么的时候再赋值吗？
            public void Coroutine() { 
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