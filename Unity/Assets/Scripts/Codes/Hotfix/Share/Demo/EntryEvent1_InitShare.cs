namespace ET {
    // 公用的相关组件的初始化：
    [Event(SceneType.Process)] // 进程上
    public class EntryEvent1_InitShare: AEvent<EventType.EntryEvent1> {
        // 【全局单例】组件：
        protected override async ETTask Run(Scene scene, EventType.EntryEvent1 args) {
            Root.Instance.Scene.AddComponent<NetThreadComponent>(); // 这个模块，是自己欠缺看丢了的。。【异步网络调用】这个模块，仍不够熟悉，狠多串联不起来
            Root.Instance.Scene.AddComponent<OpcodeTypeComponent>();
            Root.Instance.Scene.AddComponent<MessageDispatcherComponent>(); // 双端都有
            Root.Instance.Scene.AddComponent<NumericWatcherComponent>();
            Root.Instance.Scene.AddComponent<AIDispatcherComponent>();
            Root.Instance.Scene.AddComponent<ClientSceneManagerComponent>();
            await ETTask.CompletedTask;
        }
    }
}