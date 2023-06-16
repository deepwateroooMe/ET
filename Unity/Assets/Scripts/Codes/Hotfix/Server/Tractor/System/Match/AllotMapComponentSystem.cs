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
            StartConfig[] startConfigs = self.GetParent<Entity>().GetComponent<StartConfigComponent>().GetAll();// 这个组件被改了。。。
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