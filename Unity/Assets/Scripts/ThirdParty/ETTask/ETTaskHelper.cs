using System;
using System.Collections.Generic;
namespace ET {
    public static class ETTaskHelper {
        public static bool IsCancel(this ETCancellationToken self) {
            if (self == null) 
                return false;
            return self.IsDispose();
        }
        // 【看不懂】：感觉理解这个类有难度
        private class CoroutineBlocker {
            private int count; // 不知道，这个变量记的是什么？
            private ETTask tcs;
            public CoroutineBlocker(int count) {
                this.count = count;
            }
            public async ETTask RunSubCoroutineAsync(ETTask task) {
                try {
                    await task;
                }
                finally {
                    --this.count;
                    if (this.count <= 0 && this.tcs != null) { // 写结果？
                        ETTask t = this.tcs;
                        this.tcs = null;
                        t.SetResult();
                    }
                }
            }
            public async ETTask WaitAsync() {
                if (this.count <= 0) 
                    return;
                this.tcs = ETTask.Create(true);
                await tcs;
            }
        }
        public static async ETTask WaitAny(List<ETTask> tasks) {
            if (tasks.Count == 0) 
                return;
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(1);
            foreach (ETTask task in tasks) {
                coroutineBlocker.RunSubCoroutineAsync(task).Coroutine();
            }
            await coroutineBlocker.WaitAsync();
        }
        public static async ETTask WaitAny(ETTask[] tasks) {
            if (tasks.Length == 0) 
                return;
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(1);
            foreach (ETTask task in tasks) {
                coroutineBlocker.RunSubCoroutineAsync(task).Coroutine();
            }
            await coroutineBlocker.WaitAsync();
        }
        public static async ETTask WaitAll(ETTask[] tasks) {
            if (tasks.Length == 0) 
                return;
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Length);
            foreach (ETTask task in tasks) {
                coroutineBlocker.RunSubCoroutineAsync(task).Coroutine();
            }
            await coroutineBlocker.WaitAsync();
        }
        public static async ETTask WaitAll(List<ETTask> tasks) {
            if (tasks.Count == 0) 
                return;
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Count);
            foreach (ETTask task in tasks) {
                coroutineBlocker.RunSubCoroutineAsync(task).Coroutine();
            }
            await coroutineBlocker.WaitAsync();
        }
    }
}