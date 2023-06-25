using System.Collections.Generic;
namespace ET.Server {
    // 这个类：我需要去【热更新】层去定义它的热更新生成系
    [ComponentOf(typeof(Scene))]
    // 这里，不明白，什么情况下需要 IAwake(), 什么情况下不需要？就是组件没有什么必要初始化的时候，应该就不需要
    // public class LandlordsGateSessionKeyComponent : Entity, IAwake { // 【网关服】：管理当前网关下，所有用户【客户端】的会话框 
    public class LandlordsGateSessionKeyComponent : Entity, IAwake { // 【网关服】：管理当前网关下，所有用户【客户端】的会话框 
        public Dictionary<long, long> sessionKey = new Dictionary<long, long>();
    }
}