using System;
using System.Collections.Generic;

namespace ET {
    public static class ETTaskHelper {

        public static bool IsCancel(this ETCancellationToken self) {
            if (self == null) {
                return false;
            }
            return self.IsDispose();
        }
        // 私有类： 这里没有看懂，不知道是什么意思  
        private class CoroutineBlocker {

            private int count; // 它标记的是什么,与下面链表大小没有关系 像是锁的层级 ?
            private List<ETTask> tcss = new List<ETTask>();

            public CoroutineBlocker(int count) {
                this.count = count;
            }
            public async ETTask WaitAsync() {
                --this.count;
                if (this.count < 0) {
                    return;
                }
                if (this.count == 0) { // 当且仅当, == 0时,置空,并设置链表中所有任务的结果
                    List<ETTask> t = this.tcss;
                    this.tcss = null;
                    foreach (ETTask ttcs in t) {
                        ttcs.SetResult();
                    }
                    return;
                }
                ETTask tcs = ETTask.Create(true); // 创建一个新的空任务,从空任务池里去抓
                tcss.Add(tcs);
                await tcs;
            }
        }

        public static async ETTask<bool> WaitAny<T>(ETTask<T>[] tasks, ETCancellationToken cancellationToken = null) {
            if (tasks.Length == 0) {
                return false;
            }
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(2);
            foreach (ETTask<T> task in tasks) {
                RunOneTask(task).Coroutine();
            }
            async ETVoid RunOneTask(ETTask<T> task) {
                await task;
                await coroutineBlocker.WaitAsync();
            }
            await coroutineBlocker.WaitAsync();
            if (cancellationToken == null) {
                return true;
            }
            return !cancellationToken.IsCancel();
        }
        public static async ETTask<bool> WaitAny(ETTask[] tasks, ETCancellationToken cancellationToken = null) {
            if (tasks.Length == 0) {
                return false;
            }
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(2);
            foreach (ETTask task in tasks) {
                RunOneTask(task).Coroutine();
            }
            async ETVoid RunOneTask(ETTask task) {
                await task;
                await coroutineBlocker.WaitAsync();
            }
            await coroutineBlocker.WaitAsync();
            if (cancellationToken == null) {
                return true;
            }
            return !cancellationToken.IsCancel();
        }
        public static async ETTask<bool> WaitAll<T>(ETTask<T>[] tasks, ETCancellationToken cancellationToken = null) {
            if (tasks.Length == 0) {
                return false;
            }
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Length + 1);
            foreach (ETTask<T> task in tasks) {
                RunOneTask(task).Coroutine();
            }
            async ETVoid RunOneTask(ETTask<T> task) {
                await task;
                await coroutineBlocker.WaitAsync();
            }
            await coroutineBlocker.WaitAsync();
            if (cancellationToken == null) {
                return true;
            }
            return !cancellationToken.IsCancel();
        }
        public static async ETTask<bool> WaitAll<T>(List<ETTask<T>> tasks, ETCancellationToken cancellationToken = null) {
            if (tasks.Count == 0) {
                return false;
            }
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Count + 1);
            foreach (ETTask<T> task in tasks) {
                RunOneTask(task).Coroutine();
            }
            async ETVoid RunOneTask(ETTask<T> task) {
                await task;
                await coroutineBlocker.WaitAsync();
            }
            await coroutineBlocker.WaitAsync();
            if (cancellationToken == null) {
                return true;
            }
            return !cancellationToken.IsCancel();
        }
        public static async ETTask<bool> WaitAll(ETTask[] tasks, ETCancellationToken cancellationToken = null) {
            if (tasks.Length == 0) {
                return false;
            }
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Length + 1);
            foreach (ETTask task in tasks) {
                RunOneTask(task).Coroutine();
            }
            await coroutineBlocker.WaitAsync();
            async ETVoid RunOneTask(ETTask task) {
                await task;
                await coroutineBlocker.WaitAsync();
            }
            if (cancellationToken == null) {
                return true;
            }
            return !cancellationToken.IsCancel();
        }
        // 不是很懂： 弄了个并不觉得意义很大的计数器，等待链表里的异步任务一个一个地完成，然后完成后续工作        
        public static async ETTask<bool> WaitAll(List<ETTask> tasks, ETCancellationToken cancellationToken = null) {
            if (tasks.Count == 0) {
                return false;
            }
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Count + 1); // 感觉它更像是：c++指针的引用计数一样，帮助计算所有异步任务是否完成 ？
            foreach (ETTask task in tasks) {
                RunOneTask(task).Coroutine();
            }
            await coroutineBlocker.WaitAsync();
            async ETVoid RunOneTask(ETTask task) { // 内部 异步调用方法
                await task;
                await coroutineBlocker.WaitAsync();
            }
            return !cancellationToken.IsCancel();
        }
    }
}