//using System.Management.Instrumentation;
namespace ET.Client { // 本来就是要么单例 Instance, 要么生成系呀，不能把它二个揉作一团。。。
    // [ObjectSystem]
    // public class ClientComponentAwakeSystem : AwakeSystem<ClientComponent> {
    //    protected override void Awake(ClientComponent self) {
    //        self.Awake();
    //        // ClientComponent.Instance = this;  
    //        // // self.Instance = this;  
    //    }
    // }
    public class ClientComponent : Entity, IAwake { // 去找：这个组件是在哪里添加的？它说的是客户端组件，这个好像也重构了。。。
        // public static ClientComponent Instance { get; private set; }
        // public static ClientComponent Instance { get; set; }
        public User LocalPlayer { get; set; }

        // public void Awake() {
        //     Instance = this;  
        // }
    }
}