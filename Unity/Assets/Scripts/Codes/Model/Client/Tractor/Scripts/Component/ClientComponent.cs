using System.Management.Instrumentation;

namespace ET.Client {
    [ObjectSystem]
    public class ClientComponentAwakeSystem : AwakeSystem<ClientComponent> {
       protected override void Awake(ClientComponent self) {
            self.Awake();
        }
    }
    public class ClientComponent : Entity, IAwake {
        public static ClientComponent Instance { get; private set; }
        public User LocalPlayer { get; set; }
        public void Awake() {
            Instance = this;
        }
    }
}
