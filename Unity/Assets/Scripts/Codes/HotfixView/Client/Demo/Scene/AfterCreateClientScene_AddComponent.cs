namespace ET.Client {

    // 这里的意思是说：它自动化了场景创建之后的几类元件组件 的注册添加: UI元素UIComponent, UI事件机制注册UIEventComponent,以及场景资源加载组件
    // 这里体现的是组件化思想,可加载可卸载
    [Event(SceneType.Client)] // 客户端 场景创建后 事件类型
    public class AfterCreateClientScene_AddComponent: AEvent<EventType.AfterCreateClientScene> {

        protected override async ETTask Run(Scene scene, EventType.AfterCreateClientScene args) {
            scene.AddComponent<UIEventComponent>();
            scene.AddComponent<UIComponent>();
            scene.AddComponent<ResourcesLoaderComponent>();
            await ETTask.CompletedTask;
        }
    }
}