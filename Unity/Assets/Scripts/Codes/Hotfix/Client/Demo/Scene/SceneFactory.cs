using System.Net.Sockets;
namespace ET.Client { 
    public static class SceneFactory {
        
        public static async ETTask<Scene> CreateClientScene(int zone, string name) {
            await ETTask.CompletedTask;

            Scene clientScene = EntitySceneFactory.CreateScene(zone, SceneType.Client, name, ClientSceneManagerComponent.Instance);
            clientScene.AddComponent<CurrentScenesComponent>();
            clientScene.AddComponent<ObjectWait>();
            clientScene.AddComponent<PlayerComponent>(); // <<<<<<<<<<<<<<<<<<<< 【客户端】玩家小单元，为客户端场景绑定当前玩家 

            EventSystem.Instance.Publish(clientScene, new EventType.AfterCreateClientScene()); // 好奇葩的事件，去看下
            return clientScene;
        }
        public static Scene CreateCurrentScene(long id, int zone, string name, CurrentScenesComponent currentScenesComponent) {
            Scene currentScene = EntitySceneFactory.CreateScene(id, IdGenerater.Instance.GenerateInstanceId(), zone, SceneType.Current, name, currentScenesComponent);
            currentScenesComponent.Scene = currentScene;
            EventSystem.Instance.Publish(currentScene, new EventType.AfterCreateCurrentScene());
            return currentScene;
        }
    }
}