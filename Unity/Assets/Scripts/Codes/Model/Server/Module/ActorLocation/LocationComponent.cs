using System.Collections.Generic;
namespace ET.Server {
    // 这个【ActorLocation】文件夹：原本只是没有ActorLocationSenderOneType.cs 类。不曾细看【跨进程位置】相关
    // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】

    [ChildOf(typeof(LocationComponent))] // 【位置服】的组件：
    public class LockInfo: Entity, IAwake<long, CoroutineLock>, IDestroy { // 打包：协程锁的实例标记号 + 独占锁
        public long LockInstanceId;
        public CoroutineLock CoroutineLock;
    }
    [ComponentOf(typeof(Scene))]
    // 【位置组件】：去细看两个字典，所做的具体的事情。
    public class LocationComponent: Entity, IAwake { 
        public readonly Dictionary<long, long> locations = new Dictionary<long, long>();
        public readonly Dictionary<long, LockInfo> lockInfos = new Dictionary<long, LockInfo>();
    }
}