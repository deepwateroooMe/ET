using System.Collections.Generic;
using Unity.Mathematics;
namespace ET.Server {
	// 大概是个：大地图上，各区块小地图什么
	// 对于玩家，不同小地图上、视野范围内，可以看见的其它玩家，与，可以看见玩家我的所有玩家，等的管理
    [ComponentOf(typeof(Unit))]
    public class AOIEntity: Entity, IAwake<int, float3>, IDestroy {
        public Unit Unit => this.GetParent<Unit>();
        public int ViewDistance;
        private EntityRef<Cell> cell;
        public Cell Cell {
            get {
                return this.cell;
            }
            set {
                this.cell = value;
            }
        }
        // 观察进入视野的Cell
        public HashSet<long> SubEnterCells = new HashSet<long>();
        // 观察离开视野的Cell
        public HashSet<long> SubLeaveCells = new HashSet<long>();
        // 观察进入视野的Cell
        public HashSet<long> enterHashSet = new HashSet<long>();
        // 观察离开视野的Cell
        public HashSet<long> leaveHashSet = new HashSet<long>();
        // 我看的见的Unit
        public Dictionary<long, AOIEntity> SeeUnits = new Dictionary<long, AOIEntity>();
        // 看见我的Unit
        public Dictionary<long, AOIEntity> BeSeeUnits = new Dictionary<long, AOIEntity>();
        // 我看的见的Player
        public Dictionary<long, AOIEntity> SeePlayers = new Dictionary<long, AOIEntity>();
        // 看见我的Player单独放一个Dict，用于广播
        public Dictionary<long, AOIEntity> BeSeePlayers = new Dictionary<long, AOIEntity>();
    }
}