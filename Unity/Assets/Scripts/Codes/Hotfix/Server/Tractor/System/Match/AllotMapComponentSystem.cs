using ET;
using System.Linq;

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
            // StartConfig[] startConfigs = Root.Instance.Scene.GetComponent<StartConfigComponent>().GetAll();// 这个组件被改了。。。全拿的话，就把场景全部拿出来呀。。。
            StartSceneConfig [] startConfigs = StartSceneConfigCategory.Instance.GetAll().Values.ToArray();
            foreach (StartSceneConfig config in startConfigs) {
                if (config.Type != SceneType.Map) 
                    continue;
                self.MapAddress.Add(config);
            }
        }
        // 随机获取一个房间服务器地址: 这里是，随机分配，一个【地图服】的意思吧。。。这个方法是自己的写的吗？随机分配一个【匹配服】，可是得有狠多匹配服才可以随机分配，现在全局只有一个。不明白，它怎么攒了一堆【匹配服】？
        public static StartSceneConfig GetAddress(this AllotMapComponent self) {
            int n = RandomGenerator.RandomNumber(0, self.MapAddress.Count);
            return self.MapAddress[n];
        }
    }
}