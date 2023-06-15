using System.Net;
namespace ET.Server {
    // Session关联User对象组件
    // 用于Session断开时触发下线
    [ComponentOf(typeof(Session))]
    public class SessionUserComponent : Entity, IAwake, IDestroy {
        // User对象
        public User User { get; set; }
    }
}
