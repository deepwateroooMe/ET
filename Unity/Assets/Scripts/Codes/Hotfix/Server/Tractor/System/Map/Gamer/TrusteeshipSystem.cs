namespace ET.Server {
    [FriendOf(typeof(TrusteeshipComponent))]
    public static class TrusteeshipSystem {

        // 那么这里添加这个类是为什么呢，是否可以删除掉？
        [ObjectSystem]
        public class TrusteeshipComponentAwakeSystem : AwakeSystem<TrusteeshipComponent> {
            protected override void Awake(TrusteeshipComponent self) {
                //self.Awake(id);
            }
        }
    }
}
