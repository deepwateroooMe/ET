using System;
using System.Collections.Generic;
using System.Threading;
namespace ET {
    public static class FiberHelper {
        public static ActorId GetActorId(this Entity self) {
            Fiber root = self.Fiber();
            return new ActorId(root.Process, root.Id, self.InstanceId);
        }
    }
    public class Fiber: IDisposable {
        // 该字段只能框架使用，绝对不能改成public，改了后果自负
        [StaticField] // 感觉像是看懂了，可是不知道【构造器赋值】的地方在哪里【TODO】：
        [ThreadStatic]
        internal static Fiber Instance; // 【TODO】：去找，这个地方是怎么赋值的？
        public bool IsDisposed;
        public int Id;
        public int Zone;
        public Scene Root { get; }
        public Address Address {
            get {
                return new Address(this.Process, this.Id);
            }
        }
        public int Process {
            get {
                return Options.Instance.Process;
            }
        }
        public EntitySystem EntitySystem { get; }
        public Mailboxes Mailboxes { get; private set; } // 每个【纤程、场景】自带邮箱、收发邮件
        public ThreadSynchronizationContext ThreadSynchronizationContext { get; }
        public ILog Log { get; }
        private readonly Queue<ETTask> frameFinishTasks = new();
		// 用【线程】模拟【纤程】：每个纤程，需要 EntitySystem, 邮箱、纤程上下文【当线程上下文】用，其它辅助如日志等
        internal Fiber(int id, int zone, SceneType sceneType, string name) {
            this.Id = id;
            this.Zone = zone;
            this.EntitySystem = new EntitySystem();
            this.Mailboxes = new Mailboxes();
			// 每创建一个新纤程，都创建一个【异步线程的、上下文】
            this.ThreadSynchronizationContext = new ThreadSynchronizationContext();
#if UNITY
            this.Log = Logger.Instance.Log;
#else
            this.Log = new NLogger(sceneType.ToString(), this.Process, this.Id);
#endif
            this.Root = new Scene(this, id, 1, sceneType, name);
        }
        internal void Update() {
            try {
                this.EntitySystem.Update();
            }
            catch (Exception e) {
                this.Log.Error(e);
            }
        }
        internal void LateUpdate() {
            try {
                this.EntitySystem.LateUpdate();
                FrameFinishUpdate();
                
                this.ThreadSynchronizationContext.Update();
            }
            catch (Exception e) {
                Log.Error(e);
            }
        }
        public async ETTask WaitFrameFinish() {
            ETTask task = ETTask.Create(true);
            this.frameFinishTasks.Enqueue(task);
            await task;
        }
        private void FrameFinishUpdate() {
            while (this.frameFinishTasks.Count > 0) {
                ETTask task = this.frameFinishTasks.Dequeue();
                task.SetResult();
            }
        }
        public void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            this.IsDisposed = true;
            this.Root.Dispose();
        }
    }
}