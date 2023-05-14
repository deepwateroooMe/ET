namespace ET.Client {

    [Event(SceneType.Client)]
    public class ModeSelected_CreateRoomUI: AEvent<EventType.ModeSelected> {

        protected override async ETTask Run(Scene scene, EventType.ModeSelected args) {
            await UIHelper.Create(scene, UIType.TractorRoom, UILayer.Mid);
        }
    }
}