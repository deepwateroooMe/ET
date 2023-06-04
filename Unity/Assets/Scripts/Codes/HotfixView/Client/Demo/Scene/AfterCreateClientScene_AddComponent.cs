namespace ET.Client {
    [Event(SceneType.Client)]
<<<<<<< HEAD
    public class AfterCreateClientScene_AddComponent: AEvent<EventType.AfterCreateClientScene> {

        protected override async ETTask Run(Scene scene, EventType.AfterCreateClientScene args) {
=======
    public class AfterCreateClientScene_AddComponent: AEvent<Scene, EventType.AfterCreateClientScene>
    {
        protected override async ETTask Run(Scene scene, EventType.AfterCreateClientScene args)
        {
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
            scene.AddComponent<UIEventComponent>();
            scene.AddComponent<UIComponent>();
            scene.AddComponent<ResourcesLoaderComponent>();
            await ETTask.CompletedTask;
        }
    }
}