namespace ET.Server {
    // 感觉，这个类完全没有读懂
    [ComponentOf(typeof(Scene))]
    public class LocationProxyComponent: Entity, IAwake, IDestroy {
        [StaticField]
        public static LocationProxyComponent Instance;
    }
}