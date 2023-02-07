namespace ET.Client {

    [ComponentOf(typeof(Scene))]
    public class PlayerComponent: Entity, IAwake { // 信息最小化，减少带宽

        public long MyId { get; set; }
    }
}