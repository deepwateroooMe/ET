using System;
using System.Collections.Generic;
namespace ET {

    // 【多字典】：感觉这个字典，应该是不曾要求排序的。可是使用的地方，感觉它是排序的，再看一下
    // 眼睛真瞎：它继承自排序字典，: SortedDictionary<T, List<K>>, 当然是排序的！！！不看了
    public class MultiMap<T, K>: SortedDictionary<T, List<K>> {
        private readonly List<K> Empty = new List<K>();
        public void Add(T t, K k) {
            List<K> list;
            this.TryGetValue(t, out list);
            if (list == null) {
                list = new List<K>();
                this.Add(t, list);
            }
            list.Add(k);
        }
        public bool Remove(T t, K k) {
            List<K> list;
            this.TryGetValue(t, out list);
            if (list == null) {
                return false;
            }
            if (!list.Remove(k)) {
                return false;
            }
            if (list.Count == 0) {
                this.Remove(t);
            }
            return true;
        }
        // 不返回内部的list,copy一份出来
        // <param name="t"></param>
        public K[] GetAll(T t) {
            List<K> list;
            this.TryGetValue(t, out list);
            if (list == null) {
                return Array.Empty<K>();
            }
            return list.ToArray();
        }
        // 返回内部的list
        // <param name="t"></param>
        public new List<K> this[T t] {
            get {
                this.TryGetValue(t, out List<K> list);
                return list ?? Empty;
            }
        }
        public K GetOne(T t) {
            List<K> list;
            this.TryGetValue(t, out list);
            if (list != null && list.Count > 0) {
                return list[0];
            }
            return default;
        }
        public bool Contains(T t, K k) {
            List<K> list;
            this.TryGetValue(t, out list);
            if (list == null) {
                return false;
            }
            return list.Contains(k);
        }
    }
}