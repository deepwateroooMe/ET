namespace ET.Server {

    // 匹配对象: 匹配的玩家系统
    [ChildOf(typeof(MatchComponent))]
    public sealed class Matcher : Entity, IAwake<long> {
        // 用户ID（唯一）
        public long UserID { get; set; }
        // 玩家GateActorID
        public long PlayerID { get; set; }
        // 客户端与网关服务器的SessionID
        public long GateSessionID { get; set; }
    }
}
