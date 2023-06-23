using ET;
namespace ET.Server {
    public static class MatcherFactory {
        // 创建匹配对象: 
        public static Matcher Create(long playerId, long userId, long sessionId) {
            // 创建匹配玩家: 这里创建玩家，就直接 new 一个出来试试
            Matcher matcher = ComponentFactory.Create<Matcher, long>(userId); // 再去框架里翻一翻
            // 去参考项目里，读一读，ComponentFactory 是如何生产 Matcher 的？
// 【逻辑】：模仿 UnitFactory.Create() 方法，调用 Component 管理组件的基类 Entity.CreateWithId() 方法。那么需要先拿到 MatcherComponent 组件
            // 问题是：MatcherComponent 组件，不光管理申请匹配的人，还管理申请匹配所对应的房间。逻辑相对复杂一点儿
            // Matcher matcher = new 
            matcher.PlayerID = playerId;
            matcher.GateSessionID = sessionId;
            // 加入匹配队列
            MatcherComponentSystem.Add(Root.Instance.Scene.GetComponent<MatcherComponent>(), matcher);
            Log.Info($"玩家{userId}加入匹配队列");
            return matcher;
        }
    }
}
