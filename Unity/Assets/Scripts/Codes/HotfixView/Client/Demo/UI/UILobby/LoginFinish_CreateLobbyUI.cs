namespace ET.Client {

    // 这里就是现在流行的游戏大厅了吗? deepwaterooo游戏大厅里有几个什么样的游戏呢?
    [Event(SceneType.Client)]
    public class LoginFinish_CreateLobbyUI: AEvent<EventType.LoginFinish> {

        protected override async ETTask Run(Scene scene, EventType.LoginFinish args) {
            await UIHelper.Create(scene, UIType.UILobby, UILayer.Mid);
        }
    }
}
