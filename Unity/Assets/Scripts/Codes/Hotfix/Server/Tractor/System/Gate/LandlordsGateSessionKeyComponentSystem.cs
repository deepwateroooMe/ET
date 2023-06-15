using System.Collections.Generic;
namespace ET.Server {
    public static class LandlordsGateSessionKeyComponentSystem {
        public static void Add(LandlordsGateSessionKeyComponent self, long key, long userId) {
            self.sessionKey.Add(key, userId);
            TimeoutRemoveKey(self, key);
        }
        public static long Get(LandlordsGateSessionKeyComponent self, long key) {
            long userId;
            self.sessionKey.TryGetValue(key, out userId);
            return userId;
        }
        public static void Remove(LandlordsGateSessionKeyComponent self, long key) {
            self.sessionKey.Remove(key);
        }
        private static async void TimeoutRemoveKey(LandlordsGateSessionKeyComponent self, long key) {
            // 从游戏逻辑总管 Game 这里获取单例组件，暂时还不是狠理解这里，改天再改。大致组件存在字典里，可能要去字典里取元素
            await TimerComponent.Instance.WaitAsync(20000); // 这里拿这个组件：写得不对，它是一个单例类，使用单例的 Instance
            self.sessionKey.Remove(key);
        }
    }
}