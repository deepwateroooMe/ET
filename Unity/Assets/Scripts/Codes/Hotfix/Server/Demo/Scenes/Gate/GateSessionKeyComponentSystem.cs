namespace ET.Server {
    // 框架自带的类：感觉这个类好奇特，不同于其它的组件，它明明也申明了IAwake 接口，但没有实现。可能是框架跳过的地方吧。就是字典的初始化
    [FriendOf(typeof(GateSessionKeyComponent))]
    public static class GateSessionKeyComponentSystem {
        public static void Add(this GateSessionKeyComponent self, long key, string account) {
            self.sessionKey.Add(key, account);
            self.TimeoutRemoveKey(key).Coroutine();
        }
        public static string Get(this GateSessionKeyComponent self, long key) {
            string account = null;
            self.sessionKey.TryGetValue(key, out account);
            return account;
        }
        public static void Remove(this GateSessionKeyComponent self, long key) {
            self.sessionKey.Remove(key);
        }
        private static async ETTask TimeoutRemoveKey(this GateSessionKeyComponent self, long key) {
            await TimerComponent.Instance.WaitAsync(20000);
            self.sessionKey.Remove(key);
        }
    }
}