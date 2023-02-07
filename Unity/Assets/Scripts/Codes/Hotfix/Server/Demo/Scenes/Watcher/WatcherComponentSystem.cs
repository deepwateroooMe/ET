using System.Collections;
using System.Diagnostics;

namespace ET.Server {

// 感觉它是监看服务器的,每有个新的IP加进来,它就查看纪录一下,起什么作用呢?    
    [FriendOf(typeof(WatcherComponent))]
    public static class WatcherComponentSystem {

// Awake Destroy
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
            string[] localIP = NetworkHelper.GetAddressIPs();
            var processConfigs = StartProcessConfigCategory.Instance.GetAll();
            foreach (StartProcessConfig startProcessConfig in processConfigs.Values) {
                if (!WatcherHelper.IsThisMachine(startProcessConfig.InnerIP, localIP)) {
                    continue;
                }
                Process process = WatcherHelper.StartProcess(startProcessConfig.Id, createScenes);
                self.Processes.Add(startProcessConfig.Id, process);
                // 不知道服务器的众我机器中,有几台这样用功能的服务器? 它找到一台,并不退出loop? 可能存在同样的功能的好几台机器吗?
            }
        }
    }
}