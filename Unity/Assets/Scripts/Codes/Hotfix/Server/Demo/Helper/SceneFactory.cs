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
                case SceneType.Location: // 现在也没有位置服吧。。。有要求位置服处理的消息，所以要保留
                    // scene.AddComponent<LocationComponent>(); // 暂时还没有添加这个组件
                    break;
// 下面的：先去掉，太多报错，会吓死人的。。。
                case SceneType.AllServer: // 我想要自己添加这个全服：方便模仿参考项目，对必要的服务器组件进行管理 
                    scene.AddComponent<ActorMessageSenderComponent>();
                    scene.AddComponent<ActorLocationSenderComponent>();
                    scene.AddComponent<PlayerComponent>();
                    scene.AddComponent<UnitComponent>();
                    // PS：如果启动闪退有可能是服务器配置文件没有填数据库配置，请正确填写
                    // 这里需要将DBComponent的Awake注释去掉才能连接MongoDB
                    // scene.AddComponent<DBComponent>(); // 这个，就成为服务器端的一个重点，但是是最简单的重点，因为相比其它，它最容易
                    // 这里需要加上DBCacheComponent才能操作MongoDB
                    // scene.AddComponent<DBCacheComponent>();
                    // scene.AddComponent<DBProxyComponent>();
                    // scene.AddComponent<LocationComponent>();
                    // scene.AddComponent<ActorMessageDispatherComponent>();
                    scene.AddComponent<NetInnerComponent, string>(innerConfig.Address);
                    // scene.AddComponent<NetOuterComponent, string>(outerConfig.Address);
                    scene.AddComponent<LocationProxyComponent>();
                    // scene.AddComponent<AppManagerComponent>();
                    // scene.AddComponent<RealmGateAddressComponent>(); // <<<<<<<<<<<<<<<<<<<< 
                    scene.AddComponent<GateSessionKeyComponent>();
                    // scene.AddComponent<ConfigComponent>();
                    // scene.AddComponent<ServerFrameComponent>();
                    // scene.AddComponent<HttpComponent>();

// 以下是【拖拉机服务端】自定义全局组件
                    // GateGlobalComponent
                    scene.AddComponent<UserComponent>();
                    scene.AddComponent<LandlordsGateSessionKeyComponent>(); // <<<<<<<<<< 为什么这里要特制一个，同上面有什么不同？如果只是类名的不同，仅只为了客户端热更新方便吗？
                    // MapGlobalComponent
                    scene.AddComponent<RoomComponent>();
                    // MatchGlobalComponent
                    scene.AddComponent<AllotMapComponent>(); // 这里不知道，为什么也出错了？
                    scene.AddComponent<MatchComponent>();
                    scene.AddComponent<MatcherComponent>();
                    scene.AddComponent<MatchRoomComponent>();
                    // RealmGlobalComponent
                    scene.AddComponent<OnlineComponent>();
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