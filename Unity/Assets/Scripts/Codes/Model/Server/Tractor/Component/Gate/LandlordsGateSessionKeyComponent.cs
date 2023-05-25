using System.Collections.Generic;
namespace ET.Server {

    public class LandlordsGateSessionKeyComponent : Entity { // 【网关服】：管理当前网关下，所有用户【客户端】的会话框 
        private readonly Dictionary<long, long> sessionKey = new Dictionary<long, long>();
    }
}