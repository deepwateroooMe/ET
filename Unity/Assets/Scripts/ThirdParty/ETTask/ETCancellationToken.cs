using System;
using System.Collections.Generic;
namespace ET {
using System.Diagnostics.CodeAnalysis;

public class ETCancellationToken {// 管理所有的【取消】回调：因为可能不止一个取消回调，所以 HashSet 管理 
        private HashSet<Action> actions = new HashSet<Action>();
        public void Add(Action callback) {
            // 如果action是null，绝对不能添加,要抛异常，说明有协程泄漏
            // 【不喜欢这个注释，看不懂，感觉它吓唬人的。。】
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
            this.actions = null;
            try {
                foreach (Action action in runActions) {
                    action.Invoke();
                }
            }
            catch (Exception e) {
                ETTask.ExceptionHandler.Invoke(e);
            }
        }
    }
}