using ET;
namespace ET.Server {
    // // 把这个类也去掉: 因为有UnitFactory, 我重新把它们添加回来。可是这里改得可能还是不对
    // public static class GamerFactory {
    //     // 创建玩家对象
    //     public static Gamer Create(long playerId, long userId, long? id = null) {
    //         Gamer gamer = (Entity).Create<Gamer>(id ?? IdGenerater.Instance.GenerateId(), userId);
    //         // Gamer gamer = ComponentFactory.CreateWithId<Gamer, long>(id ?? IdGenerater.Instance.GenerateId(), userId);
    //         // 只是下面写的方法不对：
    //         // Gamer gamer = ComponentFactory.CreateWithId<Gamer, long>(id ?? IdGenerater.Instance.GenerateId(), userId);
    //         gamer.PlayerID = playerId;
    //         return gamer;
    //     }
    // }
}