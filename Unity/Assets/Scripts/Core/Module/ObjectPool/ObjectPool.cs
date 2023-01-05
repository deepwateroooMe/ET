using System;
using System.Collections.Generic;

namespace ET
{
    public class ObjectPool: Singleton<ObjectPool>
    {
		// Dictionary是非线程安全的类型，操作的时候需要对其进行线程安全处理，最简单的方式就是加锁(lock)。还是说,这里不涉及任何的多线程相关,更多的只是方便 一个客户端 的类型回收池 ?
		private readonly Dictionary<Type, Queue<object>> pool = new Dictionary<Type, Queue<object>>(); // readonly: 它是多线程安全吗? 不是安全的数据结构
        // 它每个键的值都是绝对一样的,没有任何差异性.所以,线程安不安全无所谓,拿得到就拿,拿不到生成一个新的,关系不大

        // 泛型类
        public T Fetch<T>() where T: class
        {
            return this.Fetch(typeof (T)) as T;
        }
        // 普通类
        public object Fetch(Type type)
        {
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue)) 
            {
                return Activator.CreateInstance(type);
            }

            if (queue.Count == 0)
            {
                return Activator.CreateInstance(type);
            }
            return queue.Dequeue();
        }

        public void Recycle(object obj)
        {
            Type type = obj.GetType();
            Queue<object> queue = null;
            if (!pool.TryGetValue(type, out queue))
            {
                queue = new Queue<object>();
                pool.Add(type, queue);
            }

            // 一种对象最大为1000个
            if (queue.Count > 1000)
            {
                return;
            }
            queue.Enqueue(obj);
        }
    }
}