using System;
using System.Collections.Generic;
namespace ET {
    // 【对象池】：这是一个极其简单的对象池
    public class ObjectPool: Singleton<ObjectPool> {
        private readonly Dictionary<Type, Queue<object>> pool = new Dictionary<Type, Queue<object>>();
        // 【泛型方法】：类
        public T Fetch<T>() where T: class {
            return this.Fetch(typeof (T)) as T;
        }
        public object Fetch(Type type) { // 【普通类型】方法 
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue)) {
                return Activator.CreateInstance(type);
            }
            if (queue.Count == 0) {
                return Activator.CreateInstance(type);
            }
            return queue.Dequeue();
        }
        public void Recycle(object obj) {
            Type type = obj.GetType();
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue)) {
                queue = new Queue<object>();
                pool.Add(type, queue);
            }
            // 一种对象最大为1000个
            if (queue.Count > 1000) {
                return;
            }
            queue.Enqueue(obj);
        }
    }
}