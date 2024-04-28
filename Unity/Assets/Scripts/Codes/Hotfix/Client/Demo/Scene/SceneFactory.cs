using System.Net.Sockets;
namespace ET.Client {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】

	// 客户端场景、加工厂
    public static class SceneFactory {
        public static async ETTask<Scene> CreateClientScene(int zone, string name) {
            await ETTask.CompletedTask;
            Scene clientScene = EntitySceneFactory.CreateScene(zone, SceneType.Client, name, ClientSceneManagerComponent.Instance);
            clientScene.AddComponent<CurrentScenesComponent>();
            clientScene.AddComponent<ObjectWait>(); // 这个组件，没有细看，不知道它有什么功能【TODO】：
            clientScene.AddComponent<PlayerComponent>();
// 【订阅者】的回调逻辑：会为客户端场景，添加必要管理组件：UIComponent 、UIEventComponent 、 ResourcesLoaderComponent 三个组件
            EventSystem.Instance.Publish(clientScene, new EventType.AfterCreateClientScene()); 
            return clientScene;
        }
        public static Scene CreateCurrentScene(long id, int zone, string name, CurrentScenesComponent currentScenesComponent) {
            Scene currentScene = EntitySceneFactory.CreateScene(id, IdGenerater.Instance.GenerateInstanceId(), zone, SceneType.Current, name, currentScenesComponent);
            currentScenesComponent.Scene = currentScene;
// 完成、订阅者自动化添加2 个公用组件：UIComponent 和 ResourcesLoaderComponent 组件
            EventSystem.Instance.Publish(currentScene, new EventType.AfterCreateCurrentScene()); 
            return currentScene;
        }
        
    }
}