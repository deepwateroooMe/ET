using System.Collections.Generic;
using MongoDB.Bson;
namespace ET.Server {

// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】

// 静态类：好像是纤进程时，向【位置服】发什么狗屁位置更新之类的
    public static class TransferHelper {

        public static async ETTask TransferAtFrameFinish(Unit unit, long sceneInstanceId, string sceneName) {
            await Game.WaitFrameFinish();
            await TransferHelper.Transfer(unit, sceneInstanceId, sceneName);
        }
        public static async ETTask Transfer(Unit unit, long sceneInstanceId, string sceneName) {
            // location加锁
            long unitId = unit.Id;
            long unitInstanceId = unit.InstanceId;
            M2M_UnitTransferRequest request = new M2M_UnitTransferRequest() {Entitys = new List<byte[]>()};
            request.OldInstanceId = unitInstanceId;
            request.Unit = unit.ToBson();
            foreach (Entity entity in unit.Components.Values) {
                if (entity is ITransfer) {
                    request.Entitys.Add(entity.ToBson());
                }
            }
            unit.Dispose();

            await LocationProxyComponent.Instance.Lock(LocationType.Unit, unitId, unitInstanceId); // 框架里一个使用用例，这个逻辑过程狠简单
            await ActorMessageSenderComponent.Instance.Call(sceneInstanceId, request);
        } // 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
    } 
}