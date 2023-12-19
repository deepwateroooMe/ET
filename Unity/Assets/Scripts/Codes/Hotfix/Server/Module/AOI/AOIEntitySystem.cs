using System.Collections.Generic;
using Unity.Mathematics;
namespace ET.Server {
[FriendOf(typeof(AOIEntity))]
    [FriendOf(typeof(Cell))]
    public static class AOIEntitySystem {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<AOIEntity, int, float3> {
            protected override void Awake(AOIEntity self, int distance, float3 pos) {
                self.ViewDistance = distance;
                self.DomainScene().GetComponent<AOIManagerComponent>().Add(self, pos.x, pos.z);
            }
        }
        [ObjectSystem]
        public class DestroySystem: DestroySystem<AOIEntity> {
            protected override void Destroy(AOIEntity self) {
                self.DomainScene().GetComponent<AOIManagerComponent>()?.Remove(self);
                self.ViewDistance = 0;
                self.SeeUnits.Clear();
                self.SeePlayers.Clear();
                self.BeSeePlayers.Clear();
                self.BeSeeUnits.Clear();
                self.SubEnterCells.Clear();
                self.SubLeaveCells.Clear();
            }
        }
        // 获取在自己视野中的对象
        public static Dictionary<long, AOIEntity> GetSeeUnits(this AOIEntity self) {
            return self.SeeUnits;
        }
        public static Dictionary<long, AOIEntity> GetBeSeePlayers(this AOIEntity self) {
            return self.BeSeePlayers;
        }
        public static Dictionary<long, AOIEntity> GetSeePlayers(this AOIEntity self) {
            return self.SeePlayers;
        }
        // cell中的unit进入self的视野：【我看见了 cell 里的一切！看看这里面，做了什么？】
        public static void SubEnter(this AOIEntity self, Cell cell) {
            cell.SubsEnterEntities.Add(self.Id, self);
            foreach (KeyValuePair<long, AOIEntity> kv in cell.AOIUnits) {
                if (kv.Key == self.Id) // 我也是，这个 cell 里的一个成员
                    continue;
                self.EnterSight(kv.Value); // <<<<<<<<<<<<<<<<<<<< 定义在下面
            }
        }
        public static void UnSubEnter(this AOIEntity self, Cell cell) {
            cell.SubsEnterEntities.Remove(self.Id);
        }
        public static void SubLeave(this AOIEntity self, Cell cell) {
            cell.SubsLeaveEntities.Add(self.Id, self);
        }
        // cell中的unit离开self的视野
        public static void UnSubLeave(this AOIEntity self, Cell cell) {
            foreach (KeyValuePair<long, AOIEntity> kv in cell.AOIUnits) {
                if (kv.Key == self.Id) {
                    continue;
                }
                self.LeaveSight(kv.Value);
            }
            cell.SubsLeaveEntities.Remove(self.Id);
        }
        // enter进入self视野
        public static void EnterSight(this AOIEntity self, AOIEntity enter) { // 代表，看见的，与被看见的两方
            // 有可能之前在Enter，后来出了Enter还在LeaveCell，这样仍然没有删除，继续进来Enter，这种情况不需要处理
			// 【上面，源】：写的标注，没看懂
            if (self.SeeUnits.ContainsKey(enter.Id)) {
                return;
            }
            if (!AOISeeCheckHelper.IsCanSee(self, enter)) {
                return;
            }
            if (self.Unit.Type == UnitType.Player) { // 我是，玩家
                if (enter.Unit.Type == UnitType.Player) { // 它，也是，玩家
                    self.SeeUnits.Add(enter.Id, enter);
                    enter.BeSeeUnits.Add(self.Id, self);
                    self.SeePlayers.Add(enter.Id, enter);
					// 我进入 cell, 看见了玩家 enter, 那么玩家 enter 也就同步看见了我！
                    enter.BeSeePlayers.Add(self.Id, self); // <<<<<<<<<<<<<<<<<<<< 框架里，2 处添加之一：
                } else { // 它是，怪兽
                    self.SeeUnits.Add(enter.Id, enter);
                    enter.BeSeeUnits.Add(self.Id, self);
                    enter.BeSeePlayers.Add(self.Id, self); // <<<<<<<<<<<<<<<<<<<< 怪兽，被我看见了。。
                }
            } else {
                if (enter.Unit.Type == UnitType.Player) {
                    self.SeeUnits.Add(enter.Id, enter);
                    enter.BeSeeUnits.Add(self.Id, self);
                    self.SeePlayers.Add(enter.Id, enter);
                }
                else {
                    self.SeeUnits.Add(enter.Id, enter);
                    enter.BeSeeUnits.Add(self.Id, self);
                }
            }
            EventSystem.Instance.Publish(self.DomainScene(), new EventType.UnitEnterSightRange() { A = self, B = enter });
        }

        // leave离开self视野
        public static void LeaveSight(this AOIEntity self, AOIEntity leave) {
            if (self.Id == leave.Id) {
                return;
            }
            if (!self.SeeUnits.ContainsKey(leave.Id)) {
                return;
            }
            self.SeeUnits.Remove(leave.Id);
            if (leave.Unit.Type == UnitType.Player) {
                self.SeePlayers.Remove(leave.Id);
            }
            leave.BeSeeUnits.Remove(self.Id);
            if (self.Unit.Type == UnitType.Player) {
                leave.BeSeePlayers.Remove(self.Id);
            }
            EventSystem.Instance.Publish(self.DomainScene(), new EventType.UnitLeaveSightRange { A = self, B = leave });
        }
        // 是否在Unit视野范围内
        public static bool IsBeSee(this AOIEntity self, long unitId) {
            return self.BeSeePlayers.ContainsKey(unitId);
        }
    }
}