using System.Collections.Generic;
namespace ET.Server {

    [ComponentOf(typeof(Scene))]
    public class ActorLocationSenderComponent: Entity, IAwake, IDestroy {
        public const long TIMEOUT_TIME = 60 * 1000; // 类似的，超时自动检测机制: 作用域为组件管理级别，所有位置消息统一超时时长
        public static ActorLocationSenderComponent Instance { get; set; } // 全局单例 
        public long CheckTimer;
    }
}