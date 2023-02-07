using System.Collections.Generic;
using System.IO;

namespace ET.Server {
    public static class MessageHelper { // 这个ACtor消息系统,还是狠昏,不知道是怎么回事

// UnitAdd, UnitRemove        
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

// 这里得搞明白:  什么情况下,用如下方法,谁向哪些人玩家广播消息,就是方法使用的上下文 ?
// 这里是unit, 它身背一个所有可以看见它的玩家的列表,那么就可以如此调用,向所有可以看见它的玩家广播,就是广播给与他有关联,同一视野中的玩家        
        public static void Broadcast(Unit unit, IActorMessage message) {
            Dictionary<long, AOIEntity> dict = unit.GetBeSeePlayers();
            // 网络底层做了优化，同一个消息不会多次序列化: 这里注释仍是注释离底层原理千里之外,不明白他在说什么,还要细挖 
            foreach (AOIEntity u in dict.Values) {
                // 那么下面,说起来, u.Unit.GetComponent<UnitGateComponent>().GateSessionActorId 就成为Gate Session下ActorId的唯一实例 ?
                ActorMessageSenderComponent.Instance.Send(u.Unit.GetComponent<UnitGateComponent>().GateSessionActorId, message);
            }
        }
        
        public static void SendToClient(Unit unit, IActorMessage message) {
            SendActor(unit.GetComponent<UnitGateComponent>().GateSessionActorId, message);
        }
        
        
        // 发送协议给ActorLocation
        // <param name="id">注册Actor的Id</param>
        // <param name="message"></param>
        public static void SendToLocationActor(long id, IActorLocationMessage message) {
            ActorLocationSenderComponent.Instance.Send(id, message);
        }
        
        // 发送协议给Actor
        // <param name="actorId">注册Actor的InstanceId</param>
        // <param name="message"></param>
        public static void SendActor(long actorId, IActorMessage message) {
            ActorMessageSenderComponent.Instance.Send(actorId, message);
        }
        
        // 发送RPC协议给Actor
        // <param name="actorId">注册Actor的InstanceId</param>
        // <param name="message"></param>
        public static async ETTask<IActorResponse> CallActor(long actorId, IActorRequest message) {
            return await ActorMessageSenderComponent.Instance.Call(actorId, message);
        }
        
        // 发送RPC协议给ActorLocation
        // <param name="id">注册Actor的Id</param>
        // <param name="message"></param>
        public static async ETTask<IActorResponse> CallLocationActor(long id, IActorLocationRequest message) {
            return await ActorLocationSenderComponent.Instance.Call(id, message);
        }
    }
}