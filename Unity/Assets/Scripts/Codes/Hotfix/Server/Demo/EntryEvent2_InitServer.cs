using System.Net;
namespace ET.Server {
    [Event(SceneType.Process)] // 【服务端】进程：启动组件. 【进程】这个粒度单位
    public class EntryEvent2_InitServer: AEvent<Scene, ET.EventType.EntryEvent2> {
		// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
		// 【进程】上：服务端的、每台物理机、每个进程上，都会添加下面的这些组件
        protected override async ETTask Run(Scene scene, ET.EventType.EntryEvent2 args) {
            // 发送普通actor消息【源】：普通actor消息，非位置相关IActorLocationXYZ... 今天强大了的亲爱的表哥的活宝妹，再看这些，小 case-a piece of cake 狠简单！
            Root.Instance.Scene.AddComponent<ActorMessageSenderComponent>();
            // 发送location actor消息: 这个，【TODO】：明天上午再看一遍，各种检测机制太多了，不知道算是怎么回事，其它都看懂了
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