using ET;

namespace ET.Server {
    public static class MatcherFactory {
        // 创建匹配对象
        public static Matcher Create(long playerId, long userId, long sessionId) {
            // 创建匹配玩家
            Matcher matcher = ComponentFactory.Create<Matcher, long>(userId);
            matcher.PlayerID = playerId;
            matcher.GateSessionID = sessionId;
            // 加入匹配队列
            Game.Scene.GetComponent<MatcherComponent>().Add(matcher);
            Log.Info($"玩家{userId}加入匹配队列");
            return matcher;
        }
    }
}
