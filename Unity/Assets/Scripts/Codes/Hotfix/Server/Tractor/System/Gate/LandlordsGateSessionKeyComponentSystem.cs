using System.Collections.Generic;
namespace ET.Server {
    public static class LandlordsGateSessionKeyComponentSystem {
        public static void Add(long key, long userId) {
            this.sessionKey.Add(key, userId);
            this.TimeoutRemoveKey(key);
        }
        public static long Get(long key) {
            long userId;
            this.sessionKey.TryGetValue(key, out userId);
            return userId;
        }
        public static void Remove(long key) {
            this.sessionKey.Remove(key);
        }
        private static async void TimeoutRemoveKey(long key) {
            // 从游戏逻辑总管 Game 这里获取单例组件，暂时还不是狠理解这里，改天再改。大致组件存在字典里，可能要去字典里取元素
            await Root.Scene.Get<TimerComponent>().WaitAsync(20000);
            this.sessionKey.Remove(key);
        }
    }
}