using System.Collections.Generic;
namespace ET.Server {
    [ChildOf(typeof(AOIManagerComponent))]
    public class Cell: Entity, IAwake, IDestroy {
        // 处在这个cell的单位
        public Dictionary<long, AOIEntity> AOIUnits = new Dictionary<long, AOIEntity>();
        // 订阅了这个Cell的进入事件: 【管理，事件N 个订阅者，的字典】
        public Dictionary<long, AOIEntity> SubsEnterEntities = new Dictionary<long, AOIEntity>();
        // 订阅了这个Cell的退出事件
        public Dictionary<long, AOIEntity> SubsLeaveEntities = new Dictionary<long, AOIEntity>();
    }
}