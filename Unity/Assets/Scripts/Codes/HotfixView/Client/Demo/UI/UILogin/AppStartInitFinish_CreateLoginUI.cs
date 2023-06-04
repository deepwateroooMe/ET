<<<<<<< HEAD
﻿namespace ET.Client {

    [Event(SceneType.Client)]
    public class AppStartInitFinish_CreateLoginUI: AEvent<EventType.AppStartInitFinish> {
        protected override async ETTask Run(Scene scene, EventType.AppStartInitFinish args) {
            await UIHelper.Create(scene, UIType.UILogin, UILayer.Mid);
        }
    }
=======
﻿namespace ET.Client
{
	[Event(SceneType.Client)]
	public class AppStartInitFinish_CreateLoginUI: AEvent<Scene, EventType.AppStartInitFinish>
	{
		protected override async ETTask Run(Scene scene, EventType.AppStartInitFinish args)
		{
			await UIHelper.Create(scene, UIType.UILogin, UILayer.Mid);
		}
	}
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
}
