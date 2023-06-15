using System.Collections.Generic;
using System.Linq;
namespace ET.Server {

    [ComponentOf(typeof(Scene))]
    public class UserComponent : Entity, IAwake { // User对象管理组件
        public Dictionary<long, User> idUsers = new Dictionary<long, User>();
    }
}
