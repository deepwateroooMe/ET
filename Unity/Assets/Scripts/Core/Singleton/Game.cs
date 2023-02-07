using System;
using System.Collections.Generic;

namespace ET {

    public static class Game {
        [StaticField]
        private static readonly Dictionary<Type, ISingleton> singletonTypes = new Dictionary<Type, ISingleton>();
        [StaticField]
        private static readonly Stack<ISingleton> singletons = new Stack<ISingleton>();
        [StaticField]
        private static readonly Queue<ISingleton> updates = new Queue<ISingleton>();
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
            singletons.Push(singleton);
            
            singleton.Register();
            if (singleton is ISingletonAwake awake) {
                awake.Awake(); // 如果它实现过该接口,就会自动调用这个回调函数
            }
            
            if (singleton is ISingletonUpdate) {
                updates.Enqueue(singleton);
            }
            
            if (singleton is ISingletonLateUpdate) {
                lateUpdates.Enqueue(singleton);
            }
        }
        public static async ETTask WaitFrameFinish() { // 这个类里,只有这个方法是,等待异步执行结果结束的,但是即便执行结束了,可能还没有设置结果,会晚些时候再设置结果
            ETTask task = ETTask.Create(true); // 从池里抓一个新的出来用
            frameFinishTask.Enqueue(task);     // 入队
// <<<<<<<<<<<<<<<<<<<< 这里是，异步等待任务的执行吗？应该是 假如开启了池,await之后不能再操作ETTask，否则可能操作到再次从池中分配出来的ETTask，产生灾难性的后果
            await task; 
		}

        public static void Update() {
            int count = updates.Count;
            while (count-- > 0) {
                ISingleton singleton = updates.Dequeue();
                if (singleton.IsDisposed()) {
                    continue;
                }
                if (singleton is not ISingletonUpdate update) {
                    continue;
                }
                
                updates.Enqueue(singleton);
                try {
                    update.Update();
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
        public static void FrameFinishUpdate() {
            while (frameFinishTask.Count > 0) {
// 为什么我会觉得这里它只是把ETTask从任务队列里取出来，并不曾真正执行过呢？它是在什么时候执行的，逻辑在哪里？前面那个异步方法调用的时候就已经开始执行了
                ETTask task = frameFinishTask.Dequeue(); 
                task.SetResult();
            }
        }
        public static void Close() {
            // 顺序反过来清理: 反过来清理才能真正清理得干净
            while (singletons.Count > 0) {
                ISingleton iSingleton = singletons.Pop();
                iSingleton.Destroy();
            }
            singletonTypes.Clear();
        }
    }
}