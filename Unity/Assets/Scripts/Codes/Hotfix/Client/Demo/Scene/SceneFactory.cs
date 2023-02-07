using System.Net.Sockets;

namespace ET.Client {
    public static class SceneFactory {

        public static async ETTask<Scene> CreateClientScene(int zone, string name) {
            await ETTask.CompletedTask;
            
            Scene clientScene = EntitySceneFactory.CreateScene(zone, SceneType.Client, name, ClientSceneManagerComponent.Instance);
            clientScene.AddComponent<CurrentScenesComponent>();
            clientScene.AddComponent<ObjectWait>();
            clientScene.AddComponent<PlayerComponent>();
            
            EventSystem.Instance.Publish(clientScene, new EventType.AfterCreateClientScene());
            return clientScene;
        }

// 它说, CurrentScenes可以加载更多更大的场景,如大世界 ?这两个区别是没有弄清楚        
        public static Scene CreateCurrentScene(long id, int zone, string name, CurrentScenesComponent currentScenesComponent) {
            Scene currentScene = EntitySceneFactory.CreateScene(id, IdGenerater.Instance.GenerateInstanceId(), zone, SceneType.Current, name, currentScenesComponent);
            currentScenesComponent.Scene = currentScene;
            
            EventSystem.Instance.Publish(currentScene, new EventType.AfterCreateCurrentScene());
            return currentScene;
        }
        
        
    }
}