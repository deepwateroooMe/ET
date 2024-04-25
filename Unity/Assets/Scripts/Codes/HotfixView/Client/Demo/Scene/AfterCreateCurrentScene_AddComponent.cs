namespace ET.Client {
    [Event(SceneType.Current)]
    public class AfterCreateCurrentScene_AddComponent: AEvent<Scene, EventType.AfterCreateCurrentScene> {

		// 仅只添加了2 个组件：UIComponent 和 ResourcesLoaderComponent 组件
        protected override async ETTask Run(Scene scene, EventType.AfterCreateCurrentScene args) {
            scene.AddComponent<UIComponent>();
            scene.AddComponent<ResourcesLoaderComponent>();
            await ETTask.CompletedTask;
        }
    }
}