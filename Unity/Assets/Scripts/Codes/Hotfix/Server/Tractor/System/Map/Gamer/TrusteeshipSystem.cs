namespace ET.Server {
    public static class TrusteeshipSystem {
        [ObjectSystem]
        public class TrusteeshipComponentAwakeSystem : AwakeSystem<TrusteeshipComponent,long> {
            protected override void Awake(TrusteeshipComponent self, long id) {
                self.Awake(id);
            }
        }
    }
}
