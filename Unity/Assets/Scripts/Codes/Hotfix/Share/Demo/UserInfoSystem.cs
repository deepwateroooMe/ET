namespace ET.Server {
    // 感觉这两个小行星生成系的类：是自己没能想出更好的解决办法的时候，去掉编译错误，推进项目的解法。但晚点儿理解更多之后，应该能够想出更好的解决办法的
    [FriendOf(typeof(User))]
    public static class UserInfoSystem {
        [ObjectSystem]
        public class UserInfoAwakeSystem : AwakeSystem<UserInfo, long> {
            protected override void Awake(UserInfo self, long a) {
                self.id = a;
            }
        }
    }
}
