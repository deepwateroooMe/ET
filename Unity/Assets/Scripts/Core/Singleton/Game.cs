using System;
using System.Collections.Generic;
namespace ET {
	// 双端静态类：是Unity 几大必要、生命周期回调的【感受、Monitor 链接】帮助类？双端几大主要回调链接的、桥梁总管
    public static class Game {
        [StaticField]
        private static readonly Dictionary<Type, ISingleton> singletonTypes = new Dictionary<Type, ISingleton>();
        [StaticField]
        private static readonly Stack<ISingleton> singletons = new Stack<ISingleton>();
        [StaticField]
        private static readonly Queue<ISingleton> updates = new Queue<ISingleton>(); // ET 框架主要起了这Unity 里的3 大回调
        [StaticField]
        private static readonly Queue<ISingleton> lateUpdates = new Queue<ISingleton>();
        [StaticField]
        private static readonly Queue<ETTask> frameFinishTask = new Queue<ETTask>();
        public static T AddSingleton<T>() where T: Singleton<T>, new() {
            T singleton = new T();
            AddSingleton(singleton);
            return singleton;
        }
        public static void AddSingleton(ISingleton singleton) {
            Type singletonType = singleton.GetType();
            if (singletonTypes.ContainsKey(singletonType)) {
                throw new Exception($"already exist singleton: {singletonType.Name}");
            }
            singletonTypes.Add(singletonType, singleton);
            singletons.Push(singleton); // 2 个方法
            singleton.Register();
            if (singleton is ISingletonAwake awake) {
                awake.Awake(); // 即刻触发调用 Awakw() 回调
            }
            if (singleton is ISingletonUpdate) { // 每桢执行一次
                updates.Enqueue(singleton);
            }
            if (singleton is ISingletonLateUpdate) { // 每桢执行一次
                lateUpdates.Enqueue(singleton);
            }
        }
        public static async ETTask WaitFrameFinish() { // 没想明白：这种ETTask 的实现方法
            ETTask task = ETTask.Create(true); // 任务池里抓个任务壳
            frameFinishTask.Enqueue(task); // 加入这个类型的任务链条里
            await task; 
        }
        public static void Update() {
            int count = updates.Count;
            while (count-- > 0) { // 遍历一遍：清除回收了的、和非Update() 的实现。。
                ISingleton singleton = updates.Dequeue();
                if (singleton.IsDisposed()) {
                    continue;
                }
				// 下面：几个意思 singleton==update
                if (singleton is not ISingletonUpdate update) { // <<<<<<<<<<<<<<<<<<<< 
                    continue;
                }
                
                updates.Enqueue(singleton); // 这一桢Update() 了；下一桢还要用，所以重新放回去进尾巴
                try {
                    update.Update(); // 这一桢的调用
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        public static void LateUpdate() {
            int count = lateUpdates.Count;
            while (count-- > 0) {
                ISingleton singleton = lateUpdates.Dequeue();
                
                if (singleton.IsDisposed()) {
                    continue;
                }
                if (singleton is not ISingletonLateUpdate lateUpdate) {
                    continue;
                }
                
                lateUpdates.Enqueue(singleton);
                try {
                    lateUpdate.LateUpdate();
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }
        public static void FrameFinishUpdate() { // 就没想明白：折腾这些空壳，是在干什么
            while (frameFinishTask.Count > 0) {
                ETTask task = frameFinishTask.Dequeue();
                task.SetResult();
            }
        }
        public static void Close() {
            // 顺序反过来清理
            while (singletons.Count > 0) {
                ISingleton iSingleton = singletons.Pop();
                iSingleton.Destroy();
            }
            singletonTypes.Clear();
        }
    }
}