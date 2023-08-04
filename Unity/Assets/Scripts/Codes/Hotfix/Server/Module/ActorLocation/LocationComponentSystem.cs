namespace ET.Server { // 现在这个类，可以轻松看懂：【两处，异步返回类型，相对生疏】
    [ObjectSystem] // 【报错：】不知道是不是抄错了，是说框架里这个标签有重复；可以对比文件，再删除一个重复标签，就可以了
    public class LockInfoAwakeSystem: AwakeSystem<LockInfo, long, CoroutineLock> {
        protected override void Awake(LockInfo self, long lockInstanceId, CoroutineLock coroutineLock) {
            self.CoroutineLock = coroutineLock;
            self.LockInstanceId = lockInstanceId;
        }
    }
    [ObjectSystem]
    public class LockInfoDestroySystem: DestroySystem<LockInfo> {
        protected override void Destroy(LockInfo self) {
            self.CoroutineLock.Dispose();
            self.LockInstanceId = 0;
        }
    }
    [FriendOf(typeof(LocationComponent))]
    [FriendOf(typeof(LockInfo))]
    public static class LocationComponentSystem {
// 添加【注册】的是：【被锁 actorId, 当前——独占锁的实例标记号】这里好像被我写错了，值，应该是，被查小伙伴所在进程的地址
        public static async ETTask Add(this LocationComponent self, long key, long instanceId) { 
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key)) {
                self.locations[key] = instanceId;
                Log.Info($"location add key: {key} instanceId: {instanceId}");
            }
        }
        // 【下线销号移除】：
        public static async ETTask Remove(this LocationComponent self, long key) {
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key)) {
                self.locations.Remove(key);
                Log.Info($"location remove key: {key}");
            }
        }
        // 【小伙伴云游】：上报、预报上锁时长，可以更长，因为没玩够还是继续被上锁，同一把锁；直到再上报解锁
        public static async ETTask Lock(this LocationComponent self, long key, long instanceId, int time = 0) { // 忘记这个Key 是什么了，是 actorId
            // 【入队列站列排号】：直到获得异步资源，标记是拿到一把【独占锁】
            CoroutineLock coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key);
            LockInfo lockInfo = self.AddChild<LockInfo, long, CoroutineLock>(instanceId, coroutineLock);
            self.lockInfos.Add(key, lockInfo); // 再封装：【被锁 actorId, lock 结构体】，封装的是真正实时正在锁着的时长、过程
            Log.Info($"location lock key: {key} instanceId: {instanceId}");
// 小伙伴云游上报：要2-617-1314 分钟【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
            if (time > 0) { 
                async ETTask TimeWaitAsync() { // 这里没懂：怎么还有个 ETTask 返回类型呢？  // <<<<<<<<<<<<<<<<<<<< 
                    long lockInfoInstanceId = lockInfo.InstanceId; // 先记下：当前被锁资源【独占锁】的实例标记号
// 【异步等待】：被要求的时长。它返回【ETTask】类型 ＝＝》这里可以决定这个内部局部异步方法的返回类型吗？
                    await TimerComponent.Instance.WaitAsync(time); 
                    if (lockInfo.InstanceId != lockInfoInstanceId) // 再检查：独占锁的实例标记号，是否变了？什么情况下，有可能会变呢？
                        return;
                    Log.Info($"location timeout unlock key: {key} instanceId: {instanceId} newInstanceId: {instanceId}");
                    self.UnLock(key, instanceId, instanceId);
                }
                TimeWaitAsync().Coroutine();
            }
        }
        public static void UnLock(this LocationComponent self, long key, long oldInstanceId, long newInstanceId) { // 解锁
            if (!self.lockInfos.TryGetValue(key, out LockInfo lockInfo)) { // 先检查几个异常
                Log.Error($"location unlock not found key: {key} {oldInstanceId}");
                return;
            }
            if (oldInstanceId != lockInfo.LockInstanceId) {
                Log.Error($"location unlock oldInstanceId is different: {key} {oldInstanceId}");
                return;
            }
            Log.Info($"location unlock key: {key} instanceId: {oldInstanceId} newInstanceId: {newInstanceId}");
            self.locations[key] = newInstanceId; // 写入小本：【被锁 actorId, 当前锁实例标记号】
            self.lockInfos.Remove(key); // 先从字典管理中移除 
            // 解锁：就是回收掉了呀
            lockInfo.Dispose();
        }
        // 这些异步返回类型，看着还理解不太顺。。。
        public static async ETTask<long> Get(this LocationComponent self, long key) { // 查询：位置信息  // <<<<<<<<<<<<<<<<<<<< 
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Location, key)) { // 挂号排队站队锁：这个步骤，是【异步】
                self.locations.TryGetValue(key, out long instanceId);
                Log.Info($"location get key: {key} instanceId: {instanceId}");
                return instanceId; // 返回 ETTask<lomg> 因为被排队挂号异步等待过
            }
        }
    }
}
// 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
// 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】