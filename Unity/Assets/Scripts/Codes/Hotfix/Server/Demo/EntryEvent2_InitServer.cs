using System.Net;
namespace ET.Server {
[Event(SceneType.Process)] // 作用于【同一进程】的服务端：同一核、同一进程，可以有多个不同的场景
    public class EntryEvent2_InitServer: AEvent<ET.EventType.EntryEvent2> {
	protected override async ETTask Run(Scene scene, ET.EventType.EntryEvent2 args) {
            // 发送普通actor消息
            Root.Instance.Scene.AddComponent<ActorMessageSenderComponent>(); // 【服务端】几个组件：现在这个组件，最熟悉
            // 自已添加：【数据库管理类组件】
            Root.Instance.Scene.AddComponent<DBManagerComponent>(); // 【服务端】几个组件：现在这个组件，最熟悉
            // 发送location actor消息
            Root.Instance.Scene.AddComponent<ActorLocationSenderComponent>(); // 【服务端】全局（同一进程）单例
            // 访问location server的组件
            Root.Instance.Scene.AddComponent<LocationProxyComponent>();
            Root.Instance.Scene.AddComponent<ActorMessageDispatcherComponent>();
            Root.Instance.Scene.AddComponent<ServerSceneManagerComponent>();
            Root.Instance.Scene.AddComponent<RobotCaseComponent>();
            Root.Instance.Scene.AddComponent<NavmeshComponent>();
            StartProcessConfig processConfig = StartProcessConfigCategory.Instance.Get(Options.Instance.Process); // 把这里，根先前某处，一个命令行，启动服务端进程的逻辑连接起来，就是【服务端】的命令行启动的过程
			switch (Options.Instance.AppType) { // 这里没弄清楚：它为什么，如此区分三种不同的进程？功能上的不同，主要服务端进程，工监进程、工具类进程
				case AppType.Server: { // 当启动一个进程的时候，如果是【服务端】进程：启动该进程下的，N 多小服场景。。。
                    Root.Instance.Scene.AddComponent<NetInnerComponent, IPEndPoint>(processConfig.InnerIPPort);
                    var processScenes = StartSceneConfigCategory.Instance.GetByProcess(Options.Instance.Process);
                    foreach (StartSceneConfig startConfig in processScenes) { // 下面的管理组件，要再看下
                        await SceneFactory.CreateServerScene(ServerSceneManagerComponent.Instance, startConfig.Id, startConfig.InstanceId, startConfig.Zone, startConfig.Name, startConfig.Type, startConfig);
                    }
                    break;
                }
                case AppType.Watcher: { // 【专用监视进程】：某台物理机上的某个核，是专职用来监视其它进程【或是场景的？】现在看到，重启至少可以以核为单位，重启某个进程【下的M 多服。。】
                    StartMachineConfig startMachineConfig = WatcherHelper.GetThisMachineConfig(); // 拿到：本监视进程，所在的物理机的机器配置
                    WatcherComponent watcherComponent = Root.Instance.Scene.AddComponent<WatcherComponent>(); // 添加监视组件
                    watcherComponent.Start(Options.Instance.CreateScenes); // 下面的方法：是监视，还是帮助真正启动并监视？是后者，是真正重启了进程与其上附生的各小服，并将进程纳入管理内容
					// 因为，它是如Linux 【守护进程】般的【监视服务器】：它监控、看管各服务器，所以它只需要内网组件，不与任何客户端交互 
                    Root.Instance.Scene.AddComponent<NetInnerComponent, IPEndPoint>(NetworkHelper.ToIPEndPoint($"{startMachineConfig.InnerIP}:{startMachineConfig.WatcherPort}"));
                    break;
                }
                case AppType.GameTool:
                    break;
            }
            if (Options.Instance.Console == 1) 
                Root.Instance.Scene.AddComponent<ConsoleComponent>();
        }
    }
}