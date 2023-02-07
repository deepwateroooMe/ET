using System.Net;

namespace ET.Server {

    [Event(SceneType.Process)]
    public class EntryEvent2_InitServer: AEvent<ET.EventType.EntryEvent2> {

        protected override async ETTask Run(Scene scene, ET.EventType.EntryEvent2 args) {
// 当它身份定位为某个服的时候:就需要为它配置添加装载相应的组件,以便它能够行驶某个服的功能             
            // 发送普通actor消息
            Root.Instance.Scene.AddComponent<ActorMessageSenderComponent>();
            // 发送location actor消息
            Root.Instance.Scene.AddComponent<ActorLocationSenderComponent>();
            // 访问location server的组件
            Root.Instance.Scene.AddComponent<LocationProxyComponent>();
            Root.Instance.Scene.AddComponent<ActorMessageDispatcherComponent>();
            Root.Instance.Scene.AddComponent<ServerSceneManagerComponent>();
            Root.Instance.Scene.AddComponent<RobotCaseComponent>();
            Root.Instance.Scene.AddComponent<NavmeshComponent>();

            StartProcessConfig processConfig = StartProcessConfigCategory.Instance.Get(Options.Instance.Process);
            switch (Options.Instance.AppType) {
                case AppType.Server: {
                    Root.Instance.Scene.AddComponent<NetInnerComponent, IPEndPoint>(processConfig.InnerIPPort);
                    var processScenes = StartSceneConfigCategory.Instance.GetByProcess(Options.Instance.Process);
                    foreach (StartSceneConfig startConfig in processScenes) {
                        await SceneFactory.CreateServerScene(ServerSceneManagerComponent.Instance, startConfig.Id, startConfig.InstanceId, startConfig.Zone, startConfig.Name,
                                                             startConfig.Type, startConfig);
                    }
                    break;
                }
                case AppType.Watcher: {
                    StartMachineConfig startMachineConfig = WatcherHelper.GetThisMachineConfig();
                    WatcherComponent watcherComponent = Root.Instance.Scene.AddComponent<WatcherComponent>();
                    watcherComponent.Start(Options.Instance.CreateScenes); // 服务器上会有几台这样的机器呢? 为什么遍历到一台,不break掉loop呢?
                    Root.Instance.Scene.AddComponent<NetInnerComponent, IPEndPoint>(NetworkHelper.ToIPEndPoint($"{startMachineConfig.InnerIP}:{startMachineConfig.WatcherPort}"));
                    break;
                }
                case AppType.GameTool:
                    break;
            }
            if (Options.Instance.Console == 1) {
                Root.Instance.Scene.AddComponent<ConsoleComponent>();
            }
        }
    }
}