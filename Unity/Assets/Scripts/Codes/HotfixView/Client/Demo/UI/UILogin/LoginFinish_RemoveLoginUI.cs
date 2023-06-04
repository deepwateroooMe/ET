<<<<<<< HEAD
﻿namespace ET.Client {
    [Event(SceneType.Client)]
    public class LoginFinish_RemoveLoginUI: AEvent<EventType.LoginFinish> {
        protected override async ETTask Run(Scene scene, EventType.LoginFinish args) {
            await UIHelper.Remove(scene, UIType.UILogin);
        }
    }
=======
﻿namespace ET.Client
{
	[Event(SceneType.Client)]
	public class LoginFinish_RemoveLoginUI: AEvent<Scene, EventType.LoginFinish>
	{
		protected override async ETTask Run(Scene scene, EventType.LoginFinish args)
		{
			await UIHelper.Remove(scene, UIType.UILogin);
		}
	}
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
}
