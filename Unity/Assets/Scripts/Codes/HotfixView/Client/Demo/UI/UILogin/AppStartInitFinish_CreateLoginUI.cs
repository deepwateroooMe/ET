namespace ET.Client {
	// 客户端，应用游戏启动完成、回调事件：加载登录界面
    [Event(SceneType.Client)]
    public class AppStartInitFinish_CreateLoginUI: AEvent<Scene, EventType.AppStartInitFinish> {
        protected override async ETTask Run(Scene scene, EventType.AppStartInitFinish args) {
            await UIHelper.Create(scene, UIType.UILogin, UILayer.Mid);
        }
    }
}
