
using System.Collections.Generic;
using System.IO;
namespace ET.Server {
    public static class MessageHelper {
        public static void NoticeUnitAdd(Unit unit, Unit sendUnit) {
			// 【OuterMessage 案例，学习一个例子】：所有【内网服务器】，想要发消息给客户端、给各【网关服】中转站，都走位置服，先查那个客户端所网关服的、进程信息
			// 想用这个 IActorMessage 作例子，来看下用法：地图服，想给客户端去个消息。OuterMessage: 是【内网服务器】发给各【网关服】中转站的消息？
            M2C_CreateUnits createUnits = new() { Units = new List<UnitInfo>() };
            createUnits.Units.Add(UnitHelper.CreateUnitInfo(sendUnit));
            MessageHelper.SendToClient(unit, createUnits);
        }
        public static void NoticeUnitRemove(Unit unit, Unit sendUnit) {
            M2C_RemoveUnits removeUnits = new() {Units = new List<long>()};
            removeUnits.Units.Add(sendUnit.Id);
            MessageHelper.SendToClient(unit, removeUnits);
        }
        public static void Broadcast(Unit unit, IActorMessage message) {
            Dictionary<long, AOIEntity> dict = unit.GetBeSeePlayers();
            // 网络底层做了优化，同一个消息不会多次序列化
            ActorLocationSenderOneType oneTypeLocationType = ActorLocationSenderComponent.Instance.Get(LocationType.Player);
            foreach (AOIEntity u in dict.Values) {
                oneTypeLocationType.Send(u.Unit.Id, message);
            }
        }
        public static void SendToClient(Unit unit, IActorMessage message) {
			// 【位置消息】的一个用例：这里是用 LocationType.Player 类型的 ActorLocationSenderOneType 上发 Send() 消息
			// 前面，当玩家用户登录【网关服】时，就已经向【位置服】注册上报过 unit.Id 的位置消息。虽然正在【纤进程】正被锁着，但是只要纤完一解锁，消息就能够到达
            ActorLocationSenderComponent.Instance.Get(LocationType.Player).Send(unit.Id, message);
        }
        
        public static void SendToLocationActor(int locationType, long id, IActorLocationMessage message) {
            ActorLocationSenderComponent.Instance.Get(locationType).Send(id, message);
        }
        // 发送协议给Actor
        // <param name="actorId">注册Actor的InstanceId</param>
        // <param name="message"></param>
        public static void SendActor(long actorId, IActorMessage message) {
            ActorMessageSenderComponent.Instance.Send(actorId, message);
        }
    }
}