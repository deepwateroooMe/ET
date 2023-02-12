using System;
using System.Collections.Generic;
namespace ET {
    // 单例：意思是说每个端（服务器，或是客户端）只有一个实例，而框架的逻辑是单线程多进程的，所以不用考虑什么多线程安全
    public class ObjectPool: Singleton<ObjectPool> {
        private readonly Dictionary<Type, Queue<object>> pool = new Dictionary<Type, Queue<object>>(); // readonly: 它是多线程安全吗? 不是安全的数据结构
        // 泛型类
        public T Fetch<T>() where T: class {
            return this.Fetch(typeof (T)) as T;
        }
        // 普通类
        public object Fetch(Type type) {
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))  {
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