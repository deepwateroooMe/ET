namespace ET {
    // 公用的相关组件的初始化：
    [Event(SceneType.Process)]
    public class EntryEvent1_InitShare: AEvent<EventType.EntryEvent1> {
        // 【全局单例】组件：
        protected override async ETTask Run(Scene scene, EventType.EntryEvent1 args) {
            Root.Instance.Scene.AddComponent<NetThreadComponent>();
            Root.Instance.Scene.AddComponent<OpcodeTypeComponent>();
            Root.Instance.Scene.AddComponent<MessageDispatcherComponent>(); // <<<<<<<<<<<<<<<<<<<< 
            Root.Instance.Scene.AddComponent<NumericWatcherComponent>();
            Root.Instance.Scene.AddComponent<AIDispatcherComponent>();
            Root.Instance.Scene.AddComponent<ClientSceneManagerComponent>();
            await ETTask.CompletedTask;
        }
    }
}