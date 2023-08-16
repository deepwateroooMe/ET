namespace ET.Client {
    [Event(SceneType.Current)] // 这个类：可以去查一下源码
    public class SceneChangeFinishEvent_CreateUIHelp : AEvent<Scene, EventType.SceneChangeFinish> {
        // public class SceneChangeFinishEvent_CreateUIHelp : AEvent<Scene, EventType.SceneChangeFinish> {

        protected override async ETTask Run(Scene scene, EventType.SceneChangeFinish args) { // 这个方法：必须要自己瓣出来，因为【参考项目】不存在这种事件机制的重构
            await UIHelper.Create(scene, UIType.UIHelp, UILayer.Mid);
        }
    }
}