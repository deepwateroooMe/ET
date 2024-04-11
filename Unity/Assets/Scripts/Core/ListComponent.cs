using System;
using System.Collections.Generic;
namespace ET {
	// 框架底层的、必要便利封装。封装了就极尽可能地减少了GC, 减少了游戏过程中GC-stopTheWorld 造成游戏卡顿、用户弃玩儿的风险了。。。
	// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
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
	// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
	// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
