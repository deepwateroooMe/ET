using UnityEngine.SceneManagement;
namespace ET.Client {
    [Event(SceneType.Client)]
    public class SceneChangeStart_AddComponent: AEvent<Scene, EventType.SceneChangeStart> {

		// 当切场景、新场景、开始加载时，这个辅助回调，就加载热更新资源包、异步加载到、新场景；并添加 OperaComponent 组件
        protected override async ETTask Run(Scene scene, EventType.SceneChangeStart args) {
            Scene currentScene = scene.CurrentScene();
            // 加载场景资源
            await ResourcesComponent.Instance.LoadBundleAsync($"{currentScene.Name}.unity3d");
            // 切换到map场景
            await SceneManager.LoadSceneAsync(currentScene.Name);
            currentScene.AddComponent<OperaComponent>();
        }
    }
}