using System;
using System.Collections.Generic;
namespace ET {
	// ETTask: 异步任务，都是壳，用来实现【网络异步调用、异步调用、流式写法、异步结果的、流式？流线返回】写源码、与自动返回结果、读结果、都比较流畅、舒畅
	// 【没看懂】：感觉这个类，看了一下午，看得昏昏的。。明天上午再看一下
    public static class ETTaskHelper { // 静态帮助类：【并接V|串接X】多个异步任务，实现了一定程度上【异步任务完成度、个数】上的、返回结果时间管控
        public static bool IsCancel(this ETCancellationToken self) {
            if (self == null) {
                return false;
            }
            return self.IsDispose();
        }
        private class CoroutineBlocker { // 设定一个协程等待层级数？
            private int count;
            private ETTask tcs;
            public CoroutineBlocker(int count) {
                this.count = count;
            }
            public async ETTask RunSubCoroutineAsync(ETTask task) { // 异步运行其它协程：运行传入参数的、异步任务并返回结果
                try {
                    await task; // 务必要等到，当前要求的任务 task 执行【哪里有执行，执行的是什么？】完毕  // <<<<<<<<<<<<<<<<<<<< 
                }
                finally {
                    --this.count;
                
// 当且仅当：阻塞数到了才写结果；阻塞数到了之后，所有存在的任务，只要执行完了，都可以即刻、返回结果？、不再阻塞？
                    if (this.count <= 0 && this.tcs != null) { 
// 当一个【过程中、被要求立即执行的、异步任务、执行完毕】：类的【当前缓存、可写结果的ETTask 实例 tcs】就被拿来直用写结果给它人了。。
                        ETTask t = this.tcs;
                        this.tcs = null; 
                        t.SetResult(); // 写结果：是只写1 个结果，还是必要逻辑完善与补充？
                    }
                }
            }
			// 【准备1 个异步任务壳】：为什么都需要准备1 个异步任务壳？
            public async ETTask WaitAsync() { // 函数的意思是：随便抓个、可用的 ETTask 来？
                if (this.count <= 0) { // 不再有：任何的阻塞效应了，形同虚设、直接返回。用来写结果的任务壳呢？
                    return;
                }
                this.tcs = ETTask.Create(true); // 创建：任务池里抓了个、新任务来。。返回类型：ETTask
                await tcs; // 【看不懂】：这里不懂，等的是，异步任务，生成准备就绪？
            }
        }
		// 【下面2 大类：等待1 个，与等待所有执行完毕】：都写且仅写1 个异步结果回去；并准备一个备用壳？
		// 【等待一个、执行完毕】：若异步任务不止1 个，剩余所有任务都还是会最终执行完，并执行完后即刻、不阻塞返回异步结果【返回吗？它们没壳、可以用来写结果了。。】
        public static async ETTask WaitAny(List<ETTask> tasks) { // 等【链条里、所有的、异步任务中】随便哪一个先执行完，并返回结果
            if (tasks.Count == 0) {
                return;
            }
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(1); // 等、且仅只等待一个，仅只写1 个结果回去
			// 【协程】：一个协程函数可以分N 段来执行；链条里 N 个异步任务，每个的第1 段是以【遍历链条】的前后顺序串行、执行到每个异步任务协程的第1 个返回点；之后的。。
            foreach (ETTask task in tasks) { // 串行V并行X异步、执行链条里的数目个、异步任务。都会执行完；结果？【结果】：是每个异步任务，状态机自动写好的吗？
                coroutineBlocker.RunSubCoroutineAsync(task).Coroutine();
            }
            await coroutineBlocker.WaitAsync(); // 等且只等、任何一个执行完并返回结果；准备一个备用壳。。
        }
        public static async ETTask WaitAny(ETTask[] tasks) {
            if (tasks.Length == 0) {
                return;
            }
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(1);
            foreach (ETTask task in tasks) {
                coroutineBlocker.RunSubCoroutineAsync(task).Coroutine();
            }
            await coroutineBlocker.WaitAsync();
        }
		// 【等待所有、执行完毕】
        public static async ETTask WaitAll(ETTask[] tasks) {
            if (tasks.Length == 0) {
                return;
            }
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Length);
            foreach (ETTask task in tasks) {
                coroutineBlocker.RunSubCoroutineAsync(task).Coroutine();
            }
            await coroutineBlocker.WaitAsync();
        }
        public static async ETTask WaitAll(List<ETTask> tasks) {
            if (tasks.Count == 0) {
                return;
            }
            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Count);
            foreach (ETTask task in tasks) {
                coroutineBlocker.RunSubCoroutineAsync(task).Coroutine();
            }
            await coroutineBlocker.WaitAsync();
        }
    }
}