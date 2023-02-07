using System.Collections.Generic;
using Unity.Mathematics;

namespace ET.Server {

    [FriendOf(typeof(MoveComponent))]
    [FriendOf(typeof(NumericComponent))]
    public static class UnitHelper {

        public static UnitInfo CreateUnitInfo(Unit unit) {
            UnitInfo unitInfo = new UnitInfo();
            NumericComponent nc = unit.GetComponent<NumericComponent>();
            unitInfo.UnitId = unit.Id;
            unitInfo.ConfigId = unit.ConfigId;
            unitInfo.Type = (int)unit.Type;
            unitInfo.Position = unit.Position;
            unitInfo.Forward = unit.Forward;
            MoveComponent moveComponent = unit.GetComponent<MoveComponent>();
            if (moveComponent != null) {
                if (!moveComponent.IsArrived()) {
                    unitInfo.MoveInfo = new MoveInfo() { Points = new List<float3>() };
                    unitInfo.MoveInfo.Points.Add(unit.Position);
                    for (int i = moveComponent.N; i < moveComponent.Targets.Count; ++i) {
                        float3 pos = moveComponent.Targets[i];
                        unitInfo.MoveInfo.Points.Add(pos);
                    }
                }
            }
            unitInfo.KV = new Dictionary<int, long>();
            foreach ((int key, long value) in nc.NumericDic) {
                unitInfo.KV.Add(key, value);
            }
            return unitInfo;
        }
        
        // 获取看见unit的玩家，主要用于广播: 这里应该是说,获取所有可以看得见unit这个玩家的玩家,就是与当前这个有联系有关系,或同一地图确实可见的所有玩家,用于向这所有与他有关可以看见它的玩家广播
        public static Dictionary<long, AOIEntity> GetBeSeePlayers(this Unit self) {
            return self.GetComponent<AOIEntity>().GetBeSeePlayers();
        }
    }
}