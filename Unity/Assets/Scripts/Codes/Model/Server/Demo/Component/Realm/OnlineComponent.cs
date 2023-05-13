using System.Collections.Generic;
namespace ET {

    // 在线组件，用于记录在线玩家
    public class OnlineComponent : Component {
        private readonly Dictionary<long, int> dictionary = new Dictionary<long, int>();
        // 添加在线玩家
        // <param name="userId"></param>
        // <param name="gateAppId"></param>
        public void Add(long userId, int gateAppId) {
            dictionary.Add(userId, gateAppId);
        }
        // 获取在线玩家网关服务器ID
        // <param name="userId"></param>
        public int Get(long userId) {
            int gateAppId;
            dictionary.TryGetValue(userId, out gateAppId);
            return gateAppId;
        }
        // 移除在线玩家
        // <param name="userId"></param>
        public void Remove(long userId) {
            dictionary.Remove(userId);
        }
    }
}
