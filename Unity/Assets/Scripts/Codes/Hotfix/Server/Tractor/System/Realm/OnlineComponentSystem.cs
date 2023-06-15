using System.Collections.Generic;
namespace ET.Server {
    [FriendOfAttribute(typeof(ET.Server.OnlineComponent))]    // 在线组件，用于记录在线玩家
    public static class OnlineComponentSystem {
        // 添加在线玩家
        public static void Add(OnlineComponent self, long userId, int gateAppId) {
            self.dictionary.Add(userId, gateAppId);
        }
        // 获取在线玩家网关服务器ID
        public static int Get(OnlineComponent self, long userId) {
            int gateAppId;
            self.dictionary.TryGetValue(userId, out gateAppId);
            return gateAppId;
        }
        // 移除在线玩家
        public static void Remove(OnlineComponent self, long userId) {
            self.dictionary.Remove(userId);
        }
    }
}
