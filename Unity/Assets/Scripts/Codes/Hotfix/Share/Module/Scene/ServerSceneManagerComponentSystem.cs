namespace ET {
    // 【服务端】管理不同场景的组件：服务端的不同场景是什么呢？是不同的SceneType 吗？感觉这些东西是，以前不曾接触过服务端的概念上的不熟悉。。。
    [FriendOf(typeof(ServerSceneManagerComponent))]
    public static class ServerSceneManagerComponentSystem {
        [ObjectSystem]
        public class ServerSceneManagerComponentAwakeSystem: AwakeSystem<ServerSceneManagerComponent> {
            protected override void Awake(ServerSceneManagerComponent self) {
                ServerSceneManagerComponent.Instance = self;
            }
        }
        [ObjectSystem]
        public class ServerSceneManagerComponentDestroySystem: DestroySystem<ServerSceneManagerComponent> {
            protected override void Destroy(ServerSceneManagerComponent self) {
                ServerSceneManagerComponent.Instance = null;
            }
        }
        public static Scene Get(this ServerSceneManagerComponent self, int id) {
            Scene scene = self.GetChild<Scene>(id); // 这里，它明明说，管理的是场景 Scene 
            return scene;
        }
        public static void Remove(this ServerSceneManagerComponent self, int id) {
            self.RemoveChild(id);
        }
    }
}