namespace ET.Server {
    // 为什么添加了这个级件，却没有生成系。它是用来帮助什么的？
    [ComponentOf(typeof(Player))]
    public class GateMapComponent: Entity, IAwake {
        public Scene Scene { get; set; }
    }
}