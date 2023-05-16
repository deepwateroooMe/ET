using System.Net;
using System.Net.Sockets;

namespace ET.Server {
    public static class SceneFactory {
        // 这里搞搞明白：这些服务器场景，也是异步任务，根据配置文件来创建的
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