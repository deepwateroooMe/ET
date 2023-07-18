using System.Collections.Generic;
namespace ET.Server {
    // 这个类：我需要去【热更新】层去定义它的热更新生成系
    [ComponentOf(typeof(Scene))]
    public class LandlordsGateSessionKeyComponent : Entity, IAwake { // 【网关服】：管理当前网关下，所有用户【客户端】的会话框 
        public Dictionary<long, long> sessionKey = new Dictionary<long, long>();
    }
}