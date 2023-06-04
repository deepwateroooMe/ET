namespace ET {
    // 公用的相关组件的初始化：
    [Event(SceneType.Process)]
<<<<<<< HEAD
    public class EntryEvent1_InitShare: AEvent<EventType.EntryEvent1> {

        protected override async ETTask Run(Scene scene, EventType.EntryEvent1 args) {
            Root.Instance.Scene.AddComponent<NetThreadComponent>();
=======
    public class EntryEvent1_InitShare: AEvent<Scene, EventType.EntryEvent1>
    {
        protected override async ETTask Run(Scene scene, EventType.EntryEvent1 args)
        {
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
            Root.Instance.Scene.AddComponent<OpcodeTypeComponent>();
            Root.Instance.Scene.AddComponent<MessageDispatcherComponent>();
            Root.Instance.Scene.AddComponent<NumericWatcherComponent>();
            Root.Instance.Scene.AddComponent<AIDispatcherComponent>();
            Root.Instance.Scene.AddComponent<ClientSceneManagerComponent>();
            await ETTask.CompletedTask;
        }
    }
}