namespace ET {
    // 不知道这个是做什么的
    [ComponentOf(typeof(Scene))]
    public class ClientSceneManagerComponent: Entity, IAwake, IDestroy {
        [StaticField]
        public static ClientSceneManagerComponent Instance;
    }
}