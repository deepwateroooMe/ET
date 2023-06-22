using ET;
namespace ET.Server {
    [ObjectSystem]
    public class AllotMapComponentStartSystem : StartSystem<AllotMapComponent> {
        protected override void Start(AllotMapComponent self) {
            self.Start();
        }
    }
    [FriendOf(typeof(AllotMapComponent))]
    public static class AllotMapComponentSystem {
        public static void Start(this AllotMapComponent self) {
            // 这里先去找，谁添加了AllotMapComponent 组件；拿到父组件的类型
            // StartConfig[] startConfigs = self.GetParent<Root>().GetComponent<StartConfigComponent>().GetAll();// 这个组件被改了。。。
            // 这里至少要找到 StartConfigComponent 的ET 框架中重构成什么样子了，才能够进一步地改呀。。各种Config 类
            StartConfig[] startConfigs = Root.Instance.Scene.GetComponent<StartConfigComponent>().GetAll();// 这个组件被改了。。。
            foreach (StartConfig config in startConfigs) {
                if (config.AppType != AppType.Map) 
                    continue;
                self.MapAddress.Add(config);
            }
        }
        // 随机获取一个房间服务器地址
        public static StartConfig GetAddress(this AllotMapComponent self) {
            int n = RandomGenerator.RandomNumber(0, self.MapAddress.Count);
            return self.MapAddress[n];
        }
    }
}