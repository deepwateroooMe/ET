namespace ET.Server {
    [FriendOf(typeof(Player))]
    public static class AccountInfoSystem {
        [ObjectSystem]
        public class AccountInfoAwakeSystem : AwakeSystem<AccountInfo, long> {
            protected override void Awake(AccountInfo self, long a) {
                self.id = a;
            }
        }
    }
}
