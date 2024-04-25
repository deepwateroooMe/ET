namespace ET.Server {
    [FriendOf(typeof(RouterNode))]
    public static class RouterNodeSystem {

		// 一个【路由节点】的：初始化与回收的重置数据
        [ObjectSystem]
        public class RouterNodeAwakeSystem: AwakeSystem<RouterNode> {
            protected override void Awake(RouterNode self) {
                long timeNow = TimeHelper.ServerNow();
                self.LastRecvInnerTime = timeNow;
                self.LastRecvOuterTime = timeNow;
                self.OuterIpEndPoint = null;
                self.InnerIpEndPoint = null;
                self.RouterSyncCount = 0;
                self.OuterConn = 0;
                self.InnerConn = 0;
            }
        }
        [ObjectSystem]
        public class RouterNodeDestroySystem: DestroySystem<RouterNode> {
            protected override void Destroy(RouterNode self) {
                self.OuterConn = 0;
                self.InnerConn = 0;
                self.LastRecvInnerTime = 0;
                self.LastRecvOuterTime = 0;
                self.OuterIpEndPoint = null;
                self.InnerIpEndPoint = null;
                self.InnerAddress = null;
                self.RouterSyncCount = 0;
                self.SyncCount = 0;
            }
        }

        public static bool CheckOuterCount(this RouterNode self, long timeNow) {
            if (self.LastCheckTime == 0) {
                self.LastCheckTime = timeNow; // 本秒内、检查的【起始、时间点】
            }
            if (timeNow - self.LastCheckTime > 1000) { // 对每秒钟的 count 有限制，每秒重置一下
                // Log.Debug($"router recv packet per second: {self.LimitCountPerSecond}");
                self.LimitCountPerSecond = 0;
                self.LastCheckTime = timeNow; // 重置、起始时间点
            }
            if (++self.LimitCountPerSecond > 1000) {
                return false;
            }
            return true;
        }
    }
}