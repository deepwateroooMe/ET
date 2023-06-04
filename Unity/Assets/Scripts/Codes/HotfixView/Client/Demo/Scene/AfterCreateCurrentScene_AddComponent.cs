namespace ET.Client {

    [Event(SceneType.Current)]
<<<<<<< HEAD
    public class AfterCreateCurrentScene_AddComponent: AEvent<EventType.AfterCreateCurrentScene> {
        protected override async ETTask Run(Scene scene, EventType.AfterCreateCurrentScene args) {
=======
    public class AfterCreateCurrentScene_AddComponent: AEvent<Scene, EventType.AfterCreateCurrentScene>
    {
        protected override async ETTask Run(Scene scene, EventType.AfterCreateCurrentScene args)
        {
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
            scene.AddComponent<UIComponent>();
            scene.AddComponent<ResourcesLoaderComponent>();
            await ETTask.CompletedTask;
        }
    }
}