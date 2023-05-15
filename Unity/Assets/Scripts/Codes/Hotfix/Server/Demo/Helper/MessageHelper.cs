using System.Collections.Generic;
using System.IO;
namespace ET.Server {
    public static class MessageHelper {
        public static void NoticeUnitAdd(Unit unit, Unit sendUnit) {
            M2C_CreateUnits createUnits = new M2C_CreateUnits() { Units = new List<UnitInfo>() };
            createUnits.Units.Add(UnitHelper.CreateUnitInfo(sendUnit));
            MessageHelper.SendToClient(unit, createUnits);
        }
        public static void NoticeUnitRemove(Unit unit, Unit sendUnit) {
            M2C_RemoveUnits removeUnits = new M2C_RemoveUnits() {Units = new List<long>()};
            removeUnits.Units.Add(sendUnit.Id);
            MessageHelper.SendToClient(unit, removeUnits);
        }
        public static void Broadcast(Unit unit, IActorMessage message) {
            Dictionary<long, AOIEntity> dict = unit.GetBeSeePlayers();
            // 网络底层做了优化，同一个消息不会多次序列化
            foreach (AOIEntity u in dict.Values) {
                ActorMessageSenderComponent.Instance.Send(u.Unit.GetComponent<UnitGateComponent>().GateSessionActorId, message);
            }
        }
        public static void SendToClient(Unit unit, IActorMessage message) {
            SendActor(unit.GetComponent<UnitGateComponent>().GateSessionActorId, message);
        }
        // 发送协议给ActorLocation
        public static void SendToLocationActor(long id, IActorLocationMessage message) {
            ActorLocationSenderComponent.Instance.Send(id, message);
        }
        // 发送协议给Actor
        public static void SendActor(long actorId, IActorMessage message) {
            ActorMessageSenderComponent.Instance.Send(actorId, message);
        }
        // 发送RPC协议给Actor
        public static async ETTask<IActorResponse> CallActor(long actorId, IActorRequest message) {
            return await ActorMessageSenderComponent.Instance.Call(actorId, message);
        }
        // 发送RPC协议给ActorLocation
        public static async ETTask<IActorResponse> CallLocationActor(long id, IActorLocationRequest message) {
            return await ActorLocationSenderComponent.Instance.Call(id, message);
        }
    }
}