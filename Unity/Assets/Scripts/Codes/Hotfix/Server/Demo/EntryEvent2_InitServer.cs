using System.Net;
namespace ET.Server {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	[Event(SceneType.Process)] // 【服务端】进程：启动组件. 【进程】这个粒度单位
    public class EntryEvent2_InitServer: AEvent<Scene, ET.EventType.EntryEvent2> {
		// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
		// 【进程】上：服务端的、每台物理机、每个进程上，都会添加下面的这些组件
        protected override async ETTask Run(Scene scene, ET.EventType.EntryEvent2 args) {
            // 发送普通actor消息【源】：普通actor消息，非位置相关IActorLocationXYZ... 今天强大了的亲爱的表哥的活宝妹，再看这些，小 case-a piece of cake 狠简单！
            Root.Instance.Scene.AddComponent<ActorMessageSenderComponent>();
            // 发送location actor消息: 这次，基本都看懂了
            Root.Instance.Scene.AddComponent<ActorLocationSenderComponent>();
            // 访问location server的组件
            Root.Instance.Scene.AddComponent<LocationProxyComponent>();
// 亲爱的表哥的活宝妹，今天晚上，看接下来的几个组件
            Root.Instance.Scene.AddComponent<ActorMessageDispatcherComponent>(); // Actor 消息派发器：感觉逻辑狠简单，就是封装、下发、交由各司其职的场景里的处理器去处理
			Root.Instance.Scene.AddComponent<ServerSceneManagerComponent>(); // 
            Root.Instance.Scene.AddComponent<RobotCaseComponent>();
            Root.Instance.Scene.AddComponent<NavmeshComponent>();
            StartProcessConfig processConfig = StartProcessConfigCategory.Instance.Get(Options.Instance.Process);
            switch (Options.Instance.AppType) { // 【服务端】启动：根据不同的进程类型，加载进程内的各场景
				case AppType.Server: { // 【服务端】：添加【内网组件】。服务端不同场景，走内网组件
                    Root.Instance.Scene.AddComponent<NetInnerComponent, IPEndPoint>(processConfig.InnerIPPort);
                    var processScenes = StartSceneConfigCategory.Instance.GetByProcess(Options.Instance.Process);
                    foreach (StartSceneConfig startConfig in processScenes) { // 遍历：同一进程中的，所有配置过的场景一一创建出来
                        await SceneFactory.CreateServerScene(ServerSceneManagerComponent.Instance, startConfig.Id, startConfig.InstanceId, startConfig.Zone, startConfig.Name,
                            startConfig.Type, startConfig);
                    }
                    break;
                }
				case AppType.Watcher: { // 这个进程：改天再看一下
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