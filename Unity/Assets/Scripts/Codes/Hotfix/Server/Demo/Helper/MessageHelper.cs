using System.Collections.Generic;
using System.IO;
namespace ET.Server {
	// 【静态帮助类】：全是静态方法。想来应该是挂载在，多进程、多场景、同一个IP 地址的？、同一台物理机【服务器】上的，所以静态
    public static class MessageHelper { // 这个帮助类的重构，是不是进一步简化了呢？重构为：由【地图服】下发命令给【客户端】，创建玩家。需要去理解【地图服】下发命令前的逻辑驱动，什么情况下。。

        public static void NoticeUnitAdd(Unit unit, Unit sendUnit) {
            M2C_CreateUnits createUnits = new M2C_CreateUnits() { Units = new List<UnitInfo>() }; // 【地图服】直接命令【客户端】，那么底层仍是借【网关服】中介吗？去找服务端的处理逻辑
            createUnits.Units.Add(UnitHelper.CreateUnitInfo(sendUnit));
            MessageHelper.SendToClient(unit, createUnits); // 发消息，转发【地图服】命令：【客户端】得创建一个新玩家，链表里只有一个要创建的新玩家。可以批量创建一条链表
        }
        public static void NoticeUnitRemove(Unit unit, Unit sendUnit) {
            M2C_RemoveUnits removeUnits = new M2C_RemoveUnits() {Units = new List<long>()};
            removeUnits.Units.Add(sendUnit.Id);
            MessageHelper.SendToClient(unit, removeUnits);
        }

        public static void Broadcast(Unit unit, IActorMessage message) {
            Dictionary<long, AOIEntity> dict = unit.GetBeSeePlayers(); // 拿，所有，可以看见当前玩家 unit 的链条。。
            // 网络底层做了优化，同一个消息不会多次序列化. 【TODO】：改天，也要把这个理解透彻
            foreach (AOIEntity u in dict.Values) {
// Send() 例子来看：1st 参数是，接收者——当前地图 cell 里【框架里地图的最小管理单位？】，可以看见我的所有玩家，是接收者 rpcIds. 广播给他们：亲爱的表哥的活宝妹，坐守这里 500 年！！
                ActorMessageSenderComponent.Instance.Send(u.Unit.GetComponent<UnitGateComponent>().GateSessionActorId, message);
            }
        }
// 发送给被下达命令需要创建玩家的【客户端】：发送仍使用的是，发送消息的玩家 unit 身上背的【与网关服中介会话框上添加的】邮箱
        public static void SendToClient(Unit unit, IActorMessage message) { 
            SendActor(unit.GetComponent<UnitGateComponent>().GateSessionActorId, message); // <<<<<<<<<<<<<<<<<<<< 
        }
        // 发送协议给ActorLocation: 【感觉这个方法，正确的概率大， 99%】
        public static void SendToLocationActor(long id, IActorLocationMessage message) {
            ActorLocationSenderComponent.Instance.Send(id, message);
        }
        // 发送协议给Actor: 【感觉这个方法，正确的概率大， 99%】，可是找不到调用这个静态方法的框架里的用例！！1 个也没有，先看别的【TODO】：
        public static void SendActor(long actorId, IActorMessage message) {
            ActorMessageSenderComponent.Instance.Send(actorId, message); // <<<<<<<<<<<<<<<<<<<< 
        }
        // 发送RPC协议给Actor
        public static async ETTask<IActorResponse> CallActor(long actorId, IActorRequest message) {
            return await ActorMessageSenderComponent.Instance.Call(actorId, message);
        }
        // 【发送RPC协议给ActorLocation: 】也可以这里倒着找调用方， id 传进来的是什么？
        public static async ETTask<IActorResponse> CallLocationActor(long id, IActorLocationRequest message) {
            return await ActorLocationSenderComponent.Instance.Call(id, message);
        }
    }
}