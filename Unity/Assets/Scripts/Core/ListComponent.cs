using System;
using System.Collections.Generic;
namespace ET {
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

三楼的那个死贱畜牲
一天不被骂，贱畜牲都会皮痒
把人恶心死
贱得要死要死不得活了。。。妈的
