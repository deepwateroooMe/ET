using System;
using System.Collections.Generic;
namespace ET {
    public class MultiMap<T, K>: SortedDictionary<T, List<K>> { // 有序字典。框架里的便利封装，都如亲爱的表哥的活宝妹的、多如牛毛的 emacs-snippets.....
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