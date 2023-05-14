using UnityEngine.SceneManagement;

namespace ET.Client {

    // 这个比较喜欢：场景切换，切换开始，可以做点什么？切换结束，可以做点什么？全成事件触发机制。任何时候，活宝妹就是一定要嫁给亲爱的表哥！！！
    [Event(SceneType.Client)]
    public class SceneChangeStart_AddComponent: AEvent<EventType.SceneChangeStart> {
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