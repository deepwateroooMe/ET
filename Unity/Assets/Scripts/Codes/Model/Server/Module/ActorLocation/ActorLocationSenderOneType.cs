using System.Collections.Generic;
namespace ET.Server {
    [ChildOf(typeof(ActorLocationSenderComponent))]
    public class ActorLocationSenderOneType: Entity, IAwake<int>, IDestroy {
        public const long TIMEOUT_TIME = 60 * 1000;
        public long CheckTimer;
        public int LocationType;
    }
    // 有个类：有个文件，是专门定义下面的这个类的
    // // [ComponentOf(typeof(Scene))] // 这个可能会需要添加到它的生成系里面去
    // public class ActorLocationSenderComponent: Entity, IAwake, IDestroy {
    //     public const long TIMEOUT_TIME = 60 * 1000;
    //     public static ActorLocationSenderComponent Instance { get; set; }
    //     public long CheckTimer;
    //     public ActorLocationSenderOneType[] ActorLocationSenderComponents = new ActorLocationSenderOneType[LocationType.Max];
    // }
}
