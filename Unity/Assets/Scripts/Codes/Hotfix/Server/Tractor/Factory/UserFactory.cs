using ET;
namespace ET.Server {
    public static class UserFactory {
        // 创建User对象
        public static User Create(long userId, long sessionId) {
            User user = ComponentFactory.Create<User, long>(userId);
            user.AddComponent<UnitGateComponent, long>(sessionId);
            UserComponentSystem.Add(Root.Instance.Scene.GetComponent<UserComponent>(), user);
            return user;
        }
    }
}