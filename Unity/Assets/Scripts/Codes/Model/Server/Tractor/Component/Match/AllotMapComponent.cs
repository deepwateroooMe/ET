using System.Collections.Generic;
namespace ET.Server {

    // 分配房间服务器组件，逻辑在AllotMapComponentSystem扩展
    public class AllotMapComponent : Component {
        public readonly List<StartConfig> MapAddress = new List<StartConfig>();
    }
}
