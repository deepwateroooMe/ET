using System.Net;
using System.Net.Sockets;

namespace ET.Server {
	// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
	
	// 静态帮助类：服务端、场景工厂，负责生产服务端场景实例
    public static class SceneFactory {
        public static async ETTask<Scene> CreateServerScene(Entity parent, long id, long instanceId, int zone, string name, SceneType sceneType, StartSceneConfig startSceneConfig = null) {
            await ETTask.CompletedTask;
            Scene scene = EntitySceneFactory.CreateScene(id, instanceId, zone, sceneType, name, parent);
			// 【服务端】任何场景：都具备收发邮件功能 
            scene.AddComponent<MailBoxComponent, MailboxType>(MailboxType.UnOrderMessageDispatcher);
            switch (scene.SceneType) {
				// 【TODO】：就是感觉，完全没看懂过，下面的2 个【网络、路由相关】的场景. 亲爱的表哥的活宝妹，今天上午就看这个！！这几个对亲爱的表哥的活宝妹目前的理解，比较困难一点儿
				case SceneType.Router: 
                    scene.AddComponent<RouterComponent, IPEndPoint, string>(startSceneConfig.OuterIPPort,
                        startSceneConfig.StartProcessConfig.InnerIP
                    );
                    break;
                case SceneType.RouterManager: // 正式发布请用CDN代替RouterManager
                    // 云服务器在防火墙那里做端口映射
                    scene.AddComponent<HttpComponent, string>($"http:// *:{startSceneConfig.OuterPort}/");
                    break;
                case SceneType.Realm:
                    scene.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.InnerIPOutPort);
                    break;
                case SceneType.Gate:
                    scene.AddComponent<NetServerComponent, IPEndPoint>(startSceneConfig.InnerIPOutPort);
                    scene.AddComponent<PlayerComponent>();
                    scene.AddComponent<GateSessionKeyComponent>();
                    break;
                case SceneType.Map:
                    scene.AddComponent<UnitComponent>();
                    scene.AddComponent<AOIManagerComponent>(); 
                    break;
                case SceneType.Location:
                    scene.AddComponent<LocationManagerComoponent>();
                    break;
                case SceneType.Robot:
                    scene.AddComponent<RobotManagerComponent>();
                    break;
				// 上面的【网络、路由、相关】看不懂，先试看下这2 个是干什么。。
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