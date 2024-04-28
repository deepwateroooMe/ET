namespace ET {
	// 【服务端】的、对【客户端】场景进行管理的 Manager:
    [FriendOf(typeof(ClientSceneManagerComponent))]
    public static class ClientSceneManagerComponentSystem {
        [ObjectSystem]
        public class ClientSceneManagerComponentAwakeSystem: AwakeSystem<ClientSceneManagerComponent> {
            protected override void Awake(ClientSceneManagerComponent self) {
                ClientSceneManagerComponent.Instance = self;
            }
        }
        [ObjectSystem]
        public class ClientSceneManagerComponentDestroySystem: DestroySystem<ClientSceneManagerComponent> {
            protected override void Destroy(ClientSceneManagerComponent self) {
                ClientSceneManagerComponent.Instance = null;
            }
        }
        public static Scene ClientScene(this Entity entity) {
            return ClientSceneManagerComponent.Instance.Get(entity.DomainZone());
        }
		// 对【客户端】场景，取是取先前添加过的一个子控件。【TODO】：【客户端】场景创建的逻辑，如何与【服务端】的这个管理者命令的？
        public static Scene Get(this ClientSceneManagerComponent self, long id) {
            Scene scene = self.GetChild<Scene>(id);
            return scene;
        }
        public static void Remove(this ClientSceneManagerComponent self, long id) {
            self.RemoveChild(id);
        }
    }
}