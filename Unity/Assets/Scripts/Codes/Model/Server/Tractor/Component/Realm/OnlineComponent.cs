using System.Collections.Generic;
namespace ET.Server {
    // 【去重】：感觉这个类，可能现框架里已经有了，名字不同，需要去确认一下，再作处理 
    // 在线组件，用于记录在线玩家
    public class OnlineComponent : Entity {
        private readonly Dictionary<long, int> dictionary = new Dictionary<long, int>();
        // 添加在线玩家
        public void Add(long userId, int gateAppId) {
            dictionary.Add(userId, gateAppId);
        }
        // 获取在线玩家网关服务器ID
        public int Get(long userId) {
            int gateAppId;
            dictionary.TryGetValue(userId, out gateAppId);
            return gateAppId;
        }
        // 移除在线玩家
        public void Remove(long userId) {
            dictionary.Remove(userId);
        }
    }
}
