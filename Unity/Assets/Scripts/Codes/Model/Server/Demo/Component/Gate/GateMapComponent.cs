namespace ET.Server {
    // 为什么添加了这个组件，却没有生成系？是因为框架不完整吗？ IAwake() 可以是在添加组件的时候，就会自动触发一次
    // 如果没有更多需要回调的（如IUpdate() 之类的桥接Unity 生命周期回调的）大概也可以不需要生成系，因为没有生成系需要定义的热更新内容。。。应该是这样的 

    [ComponentOf(typeof(Player))]
    public class GateMapComponent: Entity, IAwake {
        
        public Scene Scene { get; set; }
    }
}