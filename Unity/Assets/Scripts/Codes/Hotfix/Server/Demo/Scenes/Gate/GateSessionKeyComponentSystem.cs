﻿namespace ET.Server {
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
            await TimerComponent.Instance.WaitAsync(20000); // 超时时间为 20 秒，之后就自动删除了
            self.sessionKey.Remove(key);
        }
    }
}