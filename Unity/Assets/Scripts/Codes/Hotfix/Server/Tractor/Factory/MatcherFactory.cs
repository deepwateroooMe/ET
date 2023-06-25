using ET;
namespace ET.Server {
    public static class MatcherFactory { // 留个文件名：方便晚点儿清理掉不必要的文件
        // // 创建匹配对象: 
        // public static Matcher Create(long playerId, long userId, long sessionId) {
        //     // 创建匹配玩家: 这里创建玩家，就直接 new 一个出来试试
        //     Matcher matcher = ComponentFactory.Create<Matcher, long>(userId); // 再去框架里翻一翻: 我觉得这个类是不需要的
        //     // Matcher matcher = new 
        //     // 去参考项目里，读一读，ComponentFactory 是如何生产 Matcher 的？
        //     // 问题是：MatcherComponent 组件，不光管理申请匹配的人，还管理申请匹配所对应的房间。逻辑相对复杂一点儿
        //     matcher.PlayerID = playerId;
        //     matcher.GateSessionID = sessionId;
        //     // 加入匹配队列
        //     MatcherComponentSystem.Add(Root.Instance.Scene.GetComponent<MatcherComponent>(), matcher);
        //     Log.Info($"玩家{userId}加入匹配队列");
        //     return matcher;
        // }
    }
}
