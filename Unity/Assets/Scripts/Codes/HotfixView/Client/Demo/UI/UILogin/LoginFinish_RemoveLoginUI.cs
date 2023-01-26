namespace ET.Client {

    // 全部定义为事件机制(事件驱动?) 当登录完成,移除登录界面
    [Event(SceneType.Client)]
    public class LoginFinish_RemoveLoginUI: AEvent<EventType.LoginFinish> {

        protected override async ETTask Run(Scene scene, EventType.LoginFinish args) {
            await UIHelper.Remove(scene, UIType.UILogin);
        }
    }
}
