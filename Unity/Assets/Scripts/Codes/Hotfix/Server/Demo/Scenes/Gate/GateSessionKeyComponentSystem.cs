namespace ET.Server {
    
    // 框架自带的类：感觉这个类好奇特，不同于其它的组件，它明明也申明了IAwake 接口，但没有实现。
    // 真正运行时，它应该是会抛运行时异常的吧？可是感觉框架里，前几天修改编译错误的时候，出现过必须实现IAake() 接口实现的编译错误。而这里没有提醒。运行时留意一下
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
        // 【会话框】有效时长：框架缺省为 20 秒
        private static async ETTask TimeoutRemoveKey(this GateSessionKeyComponent self, long key) {
            await TimerComponent.Instance.WaitAsync(20000);
            self.sessionKey.Remove(key); // 一个会话框时间到后，自动回收
        }
    }
}