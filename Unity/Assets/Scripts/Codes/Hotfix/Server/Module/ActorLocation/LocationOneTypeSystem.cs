using System;
namespace ET.Server {
	// 这个生成系，感觉狠简单，除了1 个：字典外、还添加子控件
    [ObjectSystem]
    public class LockInfoAwakeSystem: AwakeSystem<LockInfo, long, CoroutineLock> {
        protected override void Awake(LockInfo self, long lockInstanceId, CoroutineLock coroutineLock) {
            self.CoroutineLock = coroutineLock;
            self.LockInstanceId = lockInstanceId;
        }
    }
    [ObjectSystem]
    public class LockInfoDestroySystem: DestroySystem<LockInfo> {
        protected override void Destroy(LockInfo self) {
            self.CoroutineLock.Dispose(); // 当 CoroutineLock 回收的时候
            self.LockInstanceId = 0; // 回收后置 0. 框架里狠多0
        }
    }
    [FriendOf(typeof(LocationOneType))]
    [FriendOf(typeof(LockInfo))]
    public static class LocationOneTypeSystem {
        [ObjectSystem]
        public class LocationOneTypeAwakeSystem: AwakeSystem<LocationOneType, int> {
            protected override void Awake(LocationOneType self, int locationType) {
                self.LocationType = locationType;
            }
        }
		// 【位置服】总管：对所有几种不同类型的位置管理的 CRUD.. 操作，管理逻辑
        public static async ETTask Add(this LocationOneType self, long key, long instanceId) {
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.Location; // 用【向位置服注册位置信息的】注册者类型，作标记
// 【位置服】上：多进程安全下，对coroutineLockType 类型的注册消息，多进程安全下的协程锁，保障位置服处理多进程位置注册数据安全
            using (await CoroutineLockComponent.Instance.Wait(coroutineLockType, key)) { 
                self.locations[key] = instanceId; // 字典里添加一条，就是注册上报位置服的逻辑过程
                Log.Info($"location add key: {key} instanceId: {instanceId}");
            }
        }
        public static async ETTask Remove(this LocationOneType self, long key) {
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.Location;
            using (await CoroutineLockComponent.Instance.Wait(coroutineLockType, key)) {
                self.locations.Remove(key);
                Log.Info($"location remove key: {key}");
            }
        }
		// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
        public static async ETTask Lock(this LocationOneType self, long key, long instanceId, int time = 0) { // 用户玩家纤进程时，就会向位置服申报上锁
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.Location;
			// 下面：coroutineLock, 锁的是、排队挂号的是，本进程位置服 coroutineLockType 队列的服务器压力，排队挂号，等到处理当前 key 的这个排队号
            CoroutineLock coroutineLock = await CoroutineLockComponent.Instance.Wait(coroutineLockType, key); // 这里一直等、等到队列前的全部处理完，轮到处理 key 用例
            LockInfo lockInfo = self.AddChild<LockInfo, long, CoroutineLock>(instanceId, coroutineLock); // 添加了子控件 LockInfo 实例
            self.lockInfos.Add(key, lockInfo); // 专职字典：只管理，所有正在上着锁的 id 
            Log.Info($"location lock key: {key} instanceId: {instanceId}");
            if (time > 0) {
                async ETTask TimeWaitAsync() {
                    long lockInfoInstanceId = lockInfo.InstanceId;
                    await TimerComponent.Instance.WaitAsync(time);
					// 如果 lockInfo 已经回收了，也就是它也一定执行过了 unlock() 的函数调用了
                    if (lockInfo.InstanceId != lockInfoInstanceId) { // 只是说，如果 lockInfo 回收了，则其lockInfo.InstanceId=0 就直接返回，没什么难度
                        return;
                    }
                    Log.Info($"location timeout unlock key: {key} instanceId: {instanceId} newInstanceId: {instanceId}");
                    self.UnLock(key, instanceId, instanceId);
                }
                TimeWaitAsync().Coroutine();
            } // 其它，缺省time=0 会一直锁，锁到，位置服收到请求解锁为止
        }
		// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
		// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
		// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
		// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
		// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
		// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
		// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
        public static void UnLock(this LocationOneType self, long key, long oldInstanceId, long newInstanceId) {
            if (!self.lockInfos.TryGetValue(key, out LockInfo lockInfo)) {
                Log.Error($"location unlock not found key: {key} {oldInstanceId}");
                return;
            }
            if (oldInstanceId != lockInfo.LockInstanceId) {
                Log.Error($"location unlock oldInstanceId is different: {key} {oldInstanceId}");
                return;
            }
            Log.Info($"location unlock key: {key} instanceId: {oldInstanceId} newInstanceId: {newInstanceId}");
            self.locations[key] = newInstanceId; // 更新：纤进程后的、新的实例号
            self.lockInfos.Remove(key); // 被锁时，专用字典：对被锁的 entityId 进行管理、解锁时就从字典里删除
            // 解锁
            lockInfo.Dispose(); // 【精华：协程锁回收，后续。。】<<<<<<<<<<<<<<<<<<<< 下面是，网络上的解释，亲爱的表哥的活宝妹，基本上都看懂了！！
			// void UnLock(long key, long oldInstanceId, long newInstanceId)：解锁，
			// 	1.新的InstanceId，将locations中的InstanceId更新为新的newInstanceId。
			// 	2.从lockInfos中获取到lockInfo，调用lockInfo的Dispose
			// 	3.从而调用CoroutineLock的Dispose，调用CoroutineLockComponent.Instance.Notify 【它说得比较跳跃而已，基本上是看懂了的】
			// 	4.从对应的lockQueue中以先进先出的方式，拿到一个tcs，并调用SetResult设置结果。从而使得在最先调用await的异步代码处进行唤醒，继续执行。
        }
        public static async ETTask<long> Get(this LocationOneType self, long key) {
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.Location;
            using (await CoroutineLockComponent.Instance.Wait(coroutineLockType, key)) {
				// ET 框架里，用户玩家登录【网关服】时，就会自动注册上报【位置服】玩家的位置。全框架，此唯一一处【注册上报位置服】但可以涵盖几乎所用使用场景
				// 【纤进程】时的逻辑稍不同：纤前先上锁；纤完后解锁，有专门类处理
                self.locations.TryGetValue(key, out long instanceId); // 字典里有就返回，没有返回缺省值
                Log.Info($"location get key: {key} instanceId: {instanceId}");
                return instanceId;
            }
        }
    }
    [FriendOf(typeof (LocationManagerComoponent))]
    public static class LocationComoponentSystem {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<LocationManagerComoponent> {
            protected override void Awake(LocationManagerComoponent self) {
                for (int i = 0; i < self.LocationOneTypes.Length; ++i) {
                    self.LocationOneTypes[i] = self.AddChild<LocationOneType, int>(i); // 作为子控件添加的
                }
            }
        }
        public static LocationOneType Get(this LocationManagerComoponent self, int locationType) {
            return self.LocationOneTypes[locationType];
        }
    }
}