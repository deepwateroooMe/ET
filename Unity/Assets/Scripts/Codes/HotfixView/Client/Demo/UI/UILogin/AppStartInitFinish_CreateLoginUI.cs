namespace ET.Client {

    // 当客户端 初始化 完成之后 事件: 创建登录界面
    [Event(SceneType.Client)]
    public class AppStartInitFinish_CreateLoginUI: AEvent<EventType.AppStartInitFinish> {

        protected override async ETTask Run(Scene scene, EventType.AppStartInitFinish args) {
            await UIHelper.Create(scene, UIType.UILogin, UILayer.Mid);
        }
    }
}
