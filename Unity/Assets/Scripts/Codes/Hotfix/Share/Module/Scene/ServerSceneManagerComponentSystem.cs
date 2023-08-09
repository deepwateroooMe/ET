namespace ET {
    // 【服务端】管理不同场景的组件：服务端的不同场景是什么呢？当然是，【各司其职的不同的SceneType 各小服】呀？各小服。。
    // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
    [FriendOf(typeof(ServerSceneManagerComponent))]  // 【服务端】与【客户端】是分别存在对应组件的，就说明我不曾上到高层来看这个模块，关于场景的管理。。。
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