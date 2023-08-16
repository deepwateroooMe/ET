using System.Linq;
using System.Collections.Generic;
// 【问题】：这个组件，怎么被自己放进了Share 双端里？这里感觉像是，热更域里的【服务端】的实现，组件一般是管理组件，就是对一堆此类型的小人物进行管理，多为服务端管理组件。。
namespace ET.Client {
    // [ComponentOf(typeof(Room))]  // 这里说，如果这个组件也作为UI 的子控件，同样要标记；而标记两个以上的，就不用再注明类型
    [ComponentOf]  // 这里说，如果这个组件也作为UI 的子控件，同样要标记；而标记两个以上的，就不用再注明类型
    public class GamerComponent : Entity, IAwake { // 这个类的意思，应该是说，这个Model 层固定层组件GamerComponent, 与它的热更域里的生成系 GamerComponentSystem 连接不起来，要活宝妹来连！！爱表哥，爱生活！！！

        public Dictionary<long, int> seats = new Dictionary<long, int>();
        public Gamer[] gamers = new Gamer[3];
        public Gamer LocalGamer { get; set; }
    }
}
