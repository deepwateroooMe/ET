namespace ET.Server {
	// 还不曾细看过【地图服】：不知道它上面什么逻辑，AOI 是什么。。
    [ComponentOf(typeof(Scene))]
    public class AOIManagerComponent: Entity, IAwake {
        public const int CellSize = 10 * 1000;
    }
}