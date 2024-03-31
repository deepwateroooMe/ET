using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace ET {
    public class ETCancellationToken {

        private HashSet<Action> actions = new HashSet<Action>(); // 管理【同一个、Taken】的所有可能的回调，可以不止1 个1 处回调
        public void Add(Action callback) {
            // 如果action是null，绝对不能添加,要抛异常，说明有协程泄漏
			// 【上面】：作者写这里，哪里有机制，保障了 callback ！＝ null 呢？下面Invoke()，真正执行【取消回调的时候】，能够检测到【回调，居然是空！】
            this.actions.Add(callback);
        }
        public void Remove(Action callback) {
            this.actions?.Remove(callback);
        }
        public bool IsDispose() {
            return this.actions == null;
        }
        public void Cancel() {
            if (this.actions == null) {
                return;
            }
            this.Invoke();
        }
        private void Invoke() {
            HashSet<Action> runActions = this.actions;
            this.actions = null; // 置空，不是 clear()
            try {
                foreach (Action action in runActions) {
                    action.Invoke();
                }
            }
            catch (Exception e) {
                ETTask.ExceptionHandler.Invoke(e); // 如果某个注册过的回调为空，会异常程序中止
            }
        }
    }
}