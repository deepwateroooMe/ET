using System.Collections.Generic;
namespace ET.Server {
    public class LandlordsGateSessionKeyComponent : Entity { // 【网关服】：管理当前网关下，所有用户【客户端】的会话框 
        private readonly Dictionary<long, long> sessionKey = new Dictionary<long, long>();
        public void Add(long key, long userId) {
            this.sessionKey.Add(key, userId);
            this.TimeoutRemoveKey(key);
        }
        public long Get(long key) {
            long userId;
            this.sessionKey.TryGetValue(key, out userId);
            return userId;
        }
        public void Remove(long key) {
            this.sessionKey.Remove(key);
        }
        private async void TimeoutRemoveKey(long key) {
            // 从游戏逻辑总管 Game 这里获取单例组件，暂时还不是狠理解这里，改天再改。大致组件存在字典里，可能要去字典里取元素
            await Game.Get(TimerComponent>()).WaitAsync(20000);
            this.sessionKey.Remove(key);
        }
    }
}
