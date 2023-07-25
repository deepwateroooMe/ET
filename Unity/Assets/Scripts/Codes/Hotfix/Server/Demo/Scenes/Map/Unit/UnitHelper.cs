using System.Collections.Generic;
using Unity.Mathematics;
namespace ET.Server {
    [FriendOf(typeof(MoveComponent))]
    [FriendOf(typeof(NumericComponent))]
    public static class UnitHelper { // 帮助创建：一个个小玩家 
        public static UnitInfo CreateUnitInfo(Unit unit) {
            UnitInfo unitInfo = new UnitInfo(); // 创建实例：把玩家携带的所有必要、有用信息，全部复制一遍
            NumericComponent nc = unit.GetComponent<NumericComponent>();
            unitInfo.UnitId = unit.Id;
            unitInfo.ConfigId = unit.ConfigId;
            unitInfo.Type = (int)unit.Type;
            unitInfo.Position = unit.Position;
            unitInfo.Forward = unit.Forward;
            MoveComponent moveComponent = unit.GetComponent<MoveComponent>(); // 玩家如果还在移动，要添加它的移动等
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
        // 获取看见unit的玩家，主要用于广播：
        // 就是同一小地图，或不同小地图上，但凡能够看见当前玩家 me 的所有其它于家。用于，玩家 me 要搬家了，必须广播其它小伙伴， me 移动走到住到别处去了。如此，才能保证所有玩家看见 me 的位置，所有能够看见 me 的玩家，所看见的是一样的
        public static Dictionary<long, AOIEntity> GetBeSeePlayers(this Unit self) {
            return self.GetComponent<AOIEntity>().GetBeSeePlayers();
        }
    }
}