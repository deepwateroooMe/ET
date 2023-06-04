namespace ET {
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
        
<<<<<<< HEAD
        public static Scene Get(this ClientSceneManagerComponent self, int id) {
=======
        public static Scene Get(this ClientSceneManagerComponent self, long id)
        {
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
            Scene scene = self.GetChild<Scene>(id);
            return scene;
        }
        
<<<<<<< HEAD
        public static void Remove(this ClientSceneManagerComponent self, int id) {
=======
        public static void Remove(this ClientSceneManagerComponent self, long id)
        {
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
            self.RemoveChild(id);
        }
    }
}