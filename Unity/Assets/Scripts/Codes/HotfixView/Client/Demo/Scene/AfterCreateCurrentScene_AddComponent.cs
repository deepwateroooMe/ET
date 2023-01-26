namespace ET.Client {

    [Event(SceneType.Current)] // 去找: Current 这里到底是什么意思呢 >?
    public class AfterCreateCurrentScene_AddComponent: AEvent<EventType.AfterCreateCurrentScene> {

        protected override async ETTask Run(Scene scene, EventType.AfterCreateCurrentScene args) {
            scene.AddComponent<UIComponent>();
            scene.AddComponent<ResourcesLoaderComponent>();
            await ETTask.CompletedTask;
        }
    }
}