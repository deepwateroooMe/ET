namespace ET.Server {
    [ComponentOf(typeof(Scene))] // 每个场景上？，有个位置服代理？组件是添加在每个核每个进程的【SceneType.Process】进程场景上；全【服务端】可以有【1-X】个【SceneType.Location 位置服】位置服可以有多个
    public class LocationProxyComponent: Entity, IAwake, IDestroy {
        [StaticField]
        public static LocationProxyComponent Instance; // 每个场景上，一个实例
    }
}