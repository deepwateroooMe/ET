namespace ET {
	// Share: 双端逻辑一样，最大限度地提取出双端公用部分，简化代码与逻辑；双端任何一端都还是需要各自添加一份的
    [Event(SceneType.Process)] // 双端进程上
    public class EntryEvent1_InitShare: AEvent<Scene, EventType.EntryEvent1> {
        protected override async ETTask Run(Scene scene, EventType.EntryEvent1 args) {
            Root.Instance.Scene.AddComponent<OpcodeTypeComponent>(); // 比较简单
// 进程级别的：消息派发器。负责把消息发送到：【其它进程】或是【本进程的、各司其职的各小服】上去，分场景管理
            Root.Instance.Scene.AddComponent<MessageDispatcherComponent>(); 
            Root.Instance.Scene.AddComponent<NumericWatcherComponent>();
            Root.Instance.Scene.AddComponent<AIDispatcherComponent>();
            Root.Instance.Scene.AddComponent<ClientSceneManagerComponent>();
            await ETTask.CompletedTask;
        }
    }
}