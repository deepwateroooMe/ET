using System.Collections.Generic;
namespace ET.Server {
    // 在线组件，用于记录在线玩家
    public static class OnlineComponentSystem {
        // 添加在线玩家
        public static void Add(long userId, int gateAppId) {
            dictionary.Add(userId, gateAppId);
        }
        // 获取在线玩家网关服务器ID
        public static int Get(long userId) {
            int gateAppId;
            dictionary.TryGetValue(userId, out gateAppId);
            return gateAppId;
        }
        // 移除在线玩家
        public static void Remove(long userId) {
            dictionary.Remove(userId);
        }
    }
}
