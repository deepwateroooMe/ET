namespace ET.Server {
    public static class UserSystem {
        [ObjectSystem]
        public class UserAwakeSystem : AwakeSystem<User,long> {
            protected override void Awake(User self, long id) {
                self.UserID = id;
            }
        }
    }
}