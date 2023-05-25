namespace ET.Server {
    public static class GamerSystem {
        [ObjectSystem]
        public class GamerAwakeSystem : AwakeSystem<Gamer,long> {
            protected override void Awake(Gamer self, long id) {
                self.UserID = id;
            }
        }
    }
}