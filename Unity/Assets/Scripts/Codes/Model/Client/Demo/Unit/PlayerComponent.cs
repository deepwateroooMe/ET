namespace ET.Client {
	// 背个、身份证：所以有个 PlayerSystem 来赋值初始化等
    [ComponentOf(typeof(Scene))]
    public class PlayerComponent: Entity, IAwake {
        public long MyId { get; set; }
    }
}