using System.Collections.Generic;
namespace ET.Server {

    [ComponentOf(typeof(Scene))]
    public class GateSessionKeyComponent : Entity, IAwake {
// 必要的初始化：热更域里的IAwake() 实现就被略过了？到时候运行的时候，再注意一下
        public readonly Dictionary<long, string> sessionKey = new Dictionary<long, string>(); 
    }
}