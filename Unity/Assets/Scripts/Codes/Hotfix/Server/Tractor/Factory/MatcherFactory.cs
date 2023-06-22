using ET;
namespace ET.Server {
    // 直接删除这个类
//     public static class MatcherFactory {
//         // // 创建匹配对象: 因为不再使用工厂类，把这个方法去掉，改前面调用创建的地方，使用 Entity 里的方法
//         // public static Matcher Create(long playerId, long userId, long sessionId) {
//         //     // 创建匹配玩家: 这里创建玩家，就直接 new 一个出来试试
//         //     // Matcher matcher = ComponentFactory.Create<Matcher, long>(userId);
//         //     Matcher matcher = new Matcher(userId);
//         //     matcher.PlayerID = playerId;
//         //     matcher.GateSessionID = sessionId;
//         //     // 加入匹配队列
//         //     MatcherComponentSystem.Add(Root.Instance.Scene.GetComponent<MatcherComponent>(), matcher);
//         //     Log.Info($"玩家{userId}加入匹配队列");
//         //     return matcher;
//         // }
//     }
}
