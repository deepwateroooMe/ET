using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace ET {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	// 亲爱的表哥的活宝妹，重点把ET8 里的重大更新：【纤程】相关，都看得懂！！
    public enum SchedulerType {
        Main,
        Thread,
        ThreadPool,
    }
    public class FiberManager: Singleton<FiberManager>, ISingletonAwake, ISingletonReverseDispose {
        private readonly IScheduler[] schedulers = new IScheduler[3]; // 对应上面3 种不同的调度机制，每种类型一个 IScheduler 实现
        private int idGenerator = 10000000; // 10000000以下为保留的用于StartSceneConfig的fiber id, 1个区配置1000个纤程，可以配置10000个区
        private ConcurrentDictionary<int, Fiber> fibers = new();
        private MainThreadScheduler mainThreadScheduler;
        public void Awake() {
            this.mainThreadScheduler = new MainThreadScheduler(this);
            this.schedulers[(int)SchedulerType.Main] = this.mainThreadScheduler;
#if ENABLE_VIEW && UNITY_EDITOR
            this.schedulers[(int)SchedulerType.Thread] = this.mainThreadScheduler;
            this.schedulers[(int)SchedulerType.ThreadPool] = this.mainThreadScheduler;
#else // 【TODO】：去看看，这2 种调度机制、细节
            this.schedulers[(int)SchedulerType.Thread] = new ThreadScheduler(this);
            this.schedulers[(int)SchedulerType.ThreadPool] = new ThreadPoolScheduler(this);
#endif
        }
        public void Update() {
            this.mainThreadScheduler.Update();
        }
        public void LateUpdate() {
            this.mainThreadScheduler.LateUpdate();
        }
        protected override void Destroy() {
            foreach (IScheduler scheduler in this.schedulers) {
                scheduler.Dispose();
            }
            foreach (var kv in this.fibers) {
                kv.Value.Dispose();
            }
            this.fibers = null;
        }
        public async ETTask<int> Create(SchedulerType schedulerType, int fiberId, int zone, SceneType sceneType, string name) {
            try {
                Fiber fiber = new(fiberId, zone, sceneType, name);
                if (!this.fibers.TryAdd(fiberId, fiber)) { // 同步字典：添加不成功，已经存在
                    throw new Exception($"same fiber already existed, if you remove, please await Remove then Create fiber! {fiberId}");
                }
                this.schedulers[(int) schedulerType].Add(fiberId);
                
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>(); // 内部 tcs: 只是用来标记，实例场景纤程的初始化工作完成了
                fiber.ThreadSynchronizationContext.Post(async () => { // 各纤程实例，在其特定纤程线程中去执行
                    try {
                        // 根据Fiber的SceneType分发Init,必须在Fiber线程中执行【源】：
						// 新添加的【Invoke((long)SceneType.XX)】FiberInit_xyz.cs 类，都分别处理各种逻辑。这里调用的是Main. 这么就【纤程】后链接到先前ET7.2 的相同部分
						// 上面，亲爱的表哥的活宝妹第一次看时，就只找了点儿逻辑，并没有真正看明白
                        await EventSystem.Instance.Invoke<FiberInit, ETTask>((long)sceneType, new FiberInit() {Fiber = fiber});
                        tcs.SetResult(true);
                    }
                    catch (Exception e) {
                        Log.Error($"init fiber fail: {sceneType} {e}");
                    }
                });
                await tcs.Task; // 等待：这个纤程创建完成
                return fiberId; // 实例场景的、纤程线程初始化，完成了后，返回纤程身份证号
            }
            catch (Exception e) {
                throw new Exception($"create fiber error: {fiberId} {sceneType}", e);
            }
        }
        public async ETTask<int> Create(SchedulerType schedulerType, int zone, SceneType sceneType, string name) {
            int fiberId = Interlocked.Increment(ref this.idGenerator);
			// 不同【场景类型】下的【纤程初始化】逻辑不一样。现在，创建不同场景实例的纤程时，也就是等实例场景类型的【纤程线程、实例场景】初始化配置完成
            return await this.Create(schedulerType, fiberId, zone, sceneType, name);
        }
        public async ETTask Remove(int id) {
            Fiber fiber = this.Get(id);
            TaskCompletionSource<bool> tcs = new();
            // 要扔到fiber线程执行，否则会出现线程竞争
            fiber.ThreadSynchronizationContext.Post(() => {
                if (this.fibers.Remove(id, out Fiber f)) {
                    f.Dispose();
                }
                tcs.SetResult(true);
            });
            await tcs.Task;
        }
        // 不允许外部调用，容易出现多线程问题, 只能通过消息通信，不允许直接获取其它Fiber引用【源】
		// 【多线程】：【TODO】：是说，一个纤程，永远只在创建它的线程内调度、不允许线程间调度？
		// 要把上面【消息通信】看懂【TODO】：
        internal Fiber Get(int id) {
            this.fibers.TryGetValue(id, out Fiber fiber);
            return fiber;
        }
        public int Count() {
            return this.fibers.Count;
        }
    }
}