namespace ET.Server {
    [ComponentOf(typeof(Scene))] // 每个场景上？，有个位置服代理？每个进程上，有个位置服，想起来应该也是合理的，再检查一下
    public class LocationProxyComponent: Entity, IAwake, IDestroy {
        [StaticField]
        public static LocationProxyComponent Instance; // 每个场景上，一个实例
    }
}