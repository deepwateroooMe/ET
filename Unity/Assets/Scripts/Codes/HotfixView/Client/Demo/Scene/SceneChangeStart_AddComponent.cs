using UnityEngine.SceneManagement;

namespace ET.Client {

    [Event(SceneType.Client)] // 客户端场景切换开始事件: 
    public class SceneChangeStart_AddComponent: AEvent<EventType.SceneChangeStart> {

        protected override async ETTask Run(Scene scene, EventType.SceneChangeStart args) {
            Scene currentScene = scene.CurrentScene();
            
            // 加载场景资源
            await ResourcesComponent.Instance.LoadBundleAsync($"{currentScene.Name}.unity3d"); // 为什么这里资源包的名字是固定的 ?
            // 切换到map场景
            await SceneManager.LoadSceneAsync(currentScene.Name);
            
            currentScene.AddComponent<OperaComponent>(); // OperaComponent: 
        }
    }
}