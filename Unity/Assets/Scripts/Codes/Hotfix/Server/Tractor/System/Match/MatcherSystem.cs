namespace ET.Server {
    public static class MatcherSystem {
        [ObjectSystem]
        public class MatcherAwakeSystem : AwakeSystem<Matcher,long> {
            protected override void Awake(Matcher self, long id) {
                self.UserID = id;
            }
        }
    }
}
