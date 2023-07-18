namespace ET.Server {
    [ComponentOf(typeof(Scene))]
    public class BenchmarkServerComponent: Entity, IAwake { // 这个类：同样没有生成系
        public int Count { get; set; }
    }
}