using System.Net;
namespace ET.Server {
    [Event(SceneType.Process)] // 【服务端】进程：启动组件
    public class EntryEvent2_InitServer: AEvent<Scene, ET.EventType.EntryEvent2> {
		// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
        protected override async ETTask Run(Scene scene, ET.EventType.EntryEvent2 args) {
            // 发送普通actor消息【源】：普通actor消息，非位置相关IActorLocationXYZ... 今天中午会再看下这几块，亲爱的表哥的活宝妹先前疑惑过的
            Root.Instance.Scene.AddComponent<ActorMessageSenderComponent>();
            // 发送location actor消息: 这个，改天再细看一下
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
                    watcherComponent.Start(Options.Instance.CreateScenes);
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