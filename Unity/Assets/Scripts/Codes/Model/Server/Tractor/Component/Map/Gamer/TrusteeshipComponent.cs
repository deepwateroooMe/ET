namespace ET.Server {
    // 托管组件，逻辑在TrusteeshipComponentSystem扩展
    [ComponentOf(typeof(Gamer))]
    public class TrusteeshipComponent : Entity, IAwake, IStart {
        public bool Playing { get; set; }

        // public override void Dispose() {
        //     if(this.IsDisposed) {
        //         return;
        //     }
        //     base.Dispose();
        //     this.Playing = false;
        // }
    }
}
