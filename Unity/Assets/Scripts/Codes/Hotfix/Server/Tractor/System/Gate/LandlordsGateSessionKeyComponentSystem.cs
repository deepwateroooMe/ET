using System.Collections.Generic;
namespace ET.Server {

    [FriendOfAttribute(typeof(ET.Server.LandlordsGateSessionKeyComponent))]
    public static class LandlordsGateSessionKeyComponentSystem {
        public static void Add(LandlordsGateSessionKeyComponent self, long key, long userId) {
            self.sessionKey.Add(key, userId);
            // TimeoutRemoveKey(self, key); // 这里是想说，等超时会移除，可以这里怎么写呢？我居然是搜不出这个方法，电脑太慢了，先放一下
        }
        public static long Get(LandlordsGateSessionKeyComponent self, long key) {
            long userId;
            self.sessionKey.TryGetValue(key, out userId);
            return userId;
        }
        public static void Remove(LandlordsGateSessionKeyComponent self, long key) {
            self.sessionKey.Remove(key);
        }

        // 下面的这个方法说：每个同户的会话框，有效期是 20 秒，等超时会移除。
        private static async ETTask TimeoutRemoveKey(LandlordsGateSessionKeyComponent self, long key) {
            // 从游戏逻辑总管 Game 这里获取单例组件，暂时还不是狠理解这里，改天再改。大致组件存在字典里，可能要去字典里取元素
            await TimerComponent.Instance.WaitAsync(20000); // 这里拿这个组件：写得不对，它是一个单例类，使用单例的 Instance
            self.sessionKey.Remove(key);
        }
    }
}