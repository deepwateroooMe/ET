using System.Net;
using System.Net.Sockets;
namespace ET.Server { // 【服务端】

    public static class SceneFactory {
        // 根据配置文件来创建的 ??? 
        // 【添加全服：】（自己加一个全服） 全服是：当所有的【服务端】场景SceneType集中在同一个进程上，这个进程（服务器）它就是一个全服了。所以我不需要再添加任何全服。之前想错了，说明先前没能想明天这个游戏框架的架构
        public static async ETTask<Scene> CreateServerScene(Entity parent, long id, long instanceId, int zone, string name, SceneType sceneType, StartSceneConfig startSceneConfig = null) {
            await ETTask.CompletedTask; // 当框架限定了这个方法的 async ETTask<Scene> 返回类型，加这句，可以骗过编译器别报错。。。
            Scene scene = EntitySceneFactory.CreateScene(id, instanceId, zone, sceneType, name, parent);
            // 任何场景：无序消息分发器，可接收消息，队列处理；【发呢？去想，网关服，转发客户端发向地图服的消息，的过程？】
            scene.AddComponent<MailBoxComponent, MailboxType>(MailboxType.UnOrderMessageDispatcher); 

            switch (scene.SceneType) {
                case SceneType.Router:
                    // 云服务器中，一般来说router要单独部署，不过大家经常放在一起，那么下面要修改
                    // startSceneConfig.OuterIPPort改成startSceneConfig.InnerIPOutPort
                    // 然后云服务器防火墙把端口映射过来
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
                case SceneType.Location: // 一个核中的某个场景，可以是【位置服】，模块功能完整
                    scene.AddComponent<LocationComponent>();
                    break;
    //                 // 【检查自己上次删除时，不小心删除的一行】                    ：这里不小心删除少了，下面的应该全部删除
                    // 那么我重构游戏，需要再考虑的是：【匹配服】等相关场景组件，就是各个游戏本身相关的小服组件，一一添加进去
    // // 以下是【拖拉机服务端】自定义全局组件
    //                 // GateGlobalComponent
    //                 scene.AddComponent<UserComponent>();
    //                 scene.AddComponent<LandlordsGateSessionKeyComponent>(); 
    //                 // MapGlobalComponent
    //                 scene.AddComponent<RoomComponent>();
    //                 // MatchGlobalComponent
    //                 scene.AddComponent<AllotMapComponent>(); 
    //                 scene.AddComponent<MatchComponent>();
    //                 scene.AddComponent<MatcherComponent>();
    //                 scene.AddComponent<MatchRoomComponent>();
    //                 // RealmGlobalComponent
    //                 scene.AddComponent<OnlineComponent>();
    //                 break;
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