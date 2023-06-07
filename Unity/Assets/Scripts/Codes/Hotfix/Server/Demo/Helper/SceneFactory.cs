using System.Net;
using System.Net.Sockets;

namespace ET.Server {
    public static class SceneFactory {
        // 这里搞搞明白：这些服务器场景，也是异步任务，根据配置文件来创建的
        // 【添加全服：】这里自己加一个全服
        public static async ETTask<Scene> CreateServerScene(Entity parent, long id, long instanceId, int zone, string name, SceneType sceneType, StartSceneConfig startSceneConfig = null) {
            await ETTask.CompletedTask;
            Scene scene = EntitySceneFactory.CreateScene(id, instanceId, zone, sceneType, name, parent);
            // 任何场景：无序消息分发器，可接收消息，队列处理；发呢？
            scene.AddComponent<MailBoxComponent, MailboxType>(MailboxType.UnOrderMessageDispatcher); // 重构？应该是对进程间消息发收的浓缩与提练

            switch (scene.SceneType) {
                case SceneType.Router:
                    scene.AddComponent<RouterComponent, IPEndPoint, string>(startSceneConfig.OuterIPPort, startSceneConfig.StartProcessConfig.InnerIP);
                    break;
                case SceneType.RouterManager: // 正式发布请用CDN代替RouterManager
                    // 云服务器在防火墙那里做端口映射
                    scene.AddComponent<HttpComponent, string>($"http:// *:{startSceneConfig.OuterPort}/");
                    break;
                case SceneType.Realm: // 注册登录服：
                    scene.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.InnerIPOutPort);
                    break;
                case SceneType.Gate:
                    scene.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.InnerIPOutPort);
                    scene.AddComponent<PlayerComponent>();
                    scene.AddComponent<GateSessionKeyComponent>();
                    break;
                // case SceneType.Match: // 我可以添加【匹配服】相关功能组件的地方。【参考项目】没有在这里添加任何组件！！自己再想下
                //     scene.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.InnerIPOutPort);
                //     scene.AddComponent<PlayerComponent>();
                //     break;
                case SceneType.Map:
                    scene.AddComponent<UnitComponent>();
                    scene.AddComponent<AOIManagerComponent>();
                    break;
                case SceneType.Location:
                    scene.AddComponent<LocationComponent>();
                    break;
                case SceneType.AllServer: // 我想要自己添加这个全服：方便模仿参考项目，对必要的服务器组件进行管理 
                    Game.Scene.AddComponent<ActorMessageSenderComponent>();
                    Game.Scene.AddComponent<ActorLocationSenderComponent>();
                    Game.Scene.AddComponent<PlayerComponent>();
                    Game.Scene.AddComponent<UnitComponent>();
                    // PS：如果启动闪退有可能是服务器配置文件没有填数据库配置，请正确填写
                    // 这里需要将DBComponent的Awake注释去掉才能连接MongoDB
                    Game.Scene.AddComponent<DBComponent>(); // 这个，就成为服务器端的一个重点，但是是最简单的重点，因为相比其它，它最容易
                    // 这里需要加上DBCacheComponent才能操作MongoDB
                    Game.Scene.AddComponent<DBCacheComponent>();
                    Game.Scene.AddComponent<DBProxyComponent>();
                    Game.Scene.AddComponent<LocationComponent>();
                    Game.Scene.AddComponent<ActorMessageDispatherComponent>();
                    Game.Scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
                    Game.Scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
                    Game.Scene.AddComponent<LocationProxyComponent>();
                    Game.Scene.AddComponent<AppManagerComponent>();
                    Game.Scene.AddComponent<RealmGateAddressComponent>(); // <<<<<<<<<<<<<<<<<<<< 
                    Game.Scene.AddComponent<GateSessionKeyComponent>();
                    Game.Scene.AddComponent<ConfigComponent>();
                    // Game.Scene.AddComponent<ServerFrameComponent>();
                    Game.Scene.AddComponent<PathfindingComponent>();
                    // Game.Scene.AddComponent<HttpComponent>();

                    // 以下是【拖拉机服务端】自定义全局组件
                    // GateGlobalComponent
                    Game.Scene.AddComponent<UserComponent>();
                    Game.Scene.AddComponent<LandlordsGateSessionKeyComponent>(); // <<<<<<<<<< 为什么这里要特制一个，同上面有什么不同？如果只是类名的不同，仅只为了客户端热更新方便吗？
                    // MapGlobalComponent
                    Game.Scene.AddComponent<RoomComponent>();
                    // MatchGlobalComponent
                    Game.Scene.AddComponent<AllotMapComponent>();
                    Game.Scene.AddComponent<MatchComponent>();
                    Game.Scene.AddComponent<MatcherComponent>();
                    Game.Scene.AddComponent<MatchRoomComponent>();
                    // RealmGlobalComponent
                    Game.Scene.AddComponent<OnlineComponent>();
                    break;
                case SceneType.Robot:
                    scene.AddComponent<RobotManagerComponent>();
                    break;
                case SceneType.BenchmarkServer:
                    scene.AddComponent<BenchmarkServerComponent>();
                    scene.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.OuterIPPort);
                    break;
                case SceneType.BenchmarkClient:
                    scene.AddComponent<BenchmarkClientComponent>();
                    break;
   
            }
            return scene;
        }
    }
}