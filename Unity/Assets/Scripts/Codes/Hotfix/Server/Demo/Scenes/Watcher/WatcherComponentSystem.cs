using System.Collections;
using System.Diagnostics;
namespace ET.Server {
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
		// 没能看太懂：总之，这里的意思是，宕机了，要重启这台服务器。重启逻辑
        public static void Start(this WatcherComponent self, int createScenes = 0) {
            string[] localIP = NetworkHelper.GetAddressIPs(); // 这里，搞不懂：这些底层方法，返回的是什么？下面一行，拿到所有进程的配置
            var processConfigs = StartProcessConfigCategory.Instance.GetAll(); // 返回的是：所有进程的、被管理字典
            // 上面：拿到了【所有核、所有进程】的配置：遍历
            foreach (StartProcessConfig startProcessConfig in processConfigs.Values) {
                if (!WatcherHelper.IsThisMachine(startProcessConfig.InnerIP, localIP)) { // 这里的判断逻辑，没能看懂
                    continue; // 上面，就搞不明白，它在判断什么。总之是，它拿到某个（特殊标准）的进程，下面会重启那个死掉的进程（进程会被杀，安卓上老杀进程。。），和它里面伴生的N 多小服。。
                }
				// 应该是，找到【宕机服务器】所在进程，相关，重启；并加入自己的管理
                Process process = WatcherHelper.StartProcess(startProcessConfig.Id, createScenes); // 命令行重启宕机进程，配置参数，启动该进程下配置过的所有小服场景
                self.Processes.Add(startProcessConfig.Id, process); // 进程加入本监控，管理体系
            }
        }
    }
}