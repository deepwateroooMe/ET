using System;
using System.Collections.Generic;
namespace ET {
    // 感觉【没必要】：框架把链表也封装成了，可以从对象池中拿，和，可以自动回收的管理
    public class ListComponent<T>: List<T>, IDisposable {
        public static ListComponent<T> Create() {
            return ObjectPool.Instance.Fetch(typeof (ListComponent<T>)) as ListComponent<T>;
        }
        public void Dispose() {
            this.Clear();
            ObjectPool.Instance.Recycle(this);
        }
    }
}