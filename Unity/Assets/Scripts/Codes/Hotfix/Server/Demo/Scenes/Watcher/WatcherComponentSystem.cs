using System.Collections;
using System.Diagnostics;
namespace ET.Server {
    // 先找：添加使用的上下文：
    [FriendOf(typeof(WatcherComponent))]
    public static class WatcherComponentSystem {
        public class WatcherComponentAwakeSystem: AwakeSystem<WatcherComponent> {
            protected override void Awake(WatcherComponent self) {
                WatcherComponent.Instance = self;
            }
        }
    
        public class WatcherComponentDestroySystem: DestroySystem<WatcherComponent> {
            protected override void Destroy(WatcherComponent self) {
                WatcherComponent.Instance = null;
            }
        }
        
        public static void Start(this WatcherComponent self, int createScenes = 0) {
            string[] localIP = NetworkHelper.GetAddressIPs(); // 这里，搞不懂：这些底层方法，返回的是什么？下面一行，拿到所有进程的配置
            var processConfigs = StartProcessConfigCategory.Instance.GetAll();
            // 上面：拿到了【所有核、所有进程】的配置：遍历
            foreach (StartProcessConfig startProcessConfig in processConfigs.Values) {
                if (!WatcherHelper.IsThisMachine(startProcessConfig.InnerIP, localIP)) {
                    continue; // 上面，就搞不明白，它在判断什么。总之是，它拿到某个（特殊标准）的进程，下面会重启那个死掉的进程（进程会被杀，安卓上老杀进程。。），和它里面伴生的N 多小服。。
                }
                Process process = WatcherHelper.StartProcess(startProcessConfig.Id, createScenes); // 命令行重启宕机进程，配置参数，启动该进程下配置过的所有小服场景
                self.Processes.Add(startProcessConfig.Id, process); // 进程加入本监控，管理体系
            }
        }
    }
}