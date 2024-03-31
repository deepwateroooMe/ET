using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace ET {
	[AsyncMethodBuilder(typeof (AsyncETTaskCompletedMethodBuilder))]
	public struct ETTaskCompleted: ICriticalNotifyCompletion { // 【占位符、结构体】：特殊场景下，用来快速返回异步结果的？

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
        public void OnCompleted(Action continuation) { // 想找到：封装里，哪里是注册这个回调的地方？
        }
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action continuation) { // 接受：注册回调函数的地方
        }
    }
}