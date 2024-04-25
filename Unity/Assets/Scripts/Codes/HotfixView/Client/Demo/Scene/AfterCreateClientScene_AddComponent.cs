namespace ET.Client {
    [Event(SceneType.Client)]
    public class AfterCreateClientScene_AddComponent: AEvent<Scene, EventType.AfterCreateClientScene> {
		// 添加 3 个公用组件
        protected override async ETTask Run(Scene scene, EventType.AfterCreateClientScene args) {
            scene.AddComponent<UIEventComponent>();
            scene.AddComponent<UIComponent>();
            scene.AddComponent<ResourcesLoaderComponent>();
            await ETTask.CompletedTask;
        }
    }
}