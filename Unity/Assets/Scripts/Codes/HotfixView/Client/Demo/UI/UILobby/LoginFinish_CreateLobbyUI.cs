<<<<<<< HEAD
﻿namespace ET.Client {

    [Event(SceneType.Client)]
    public class LoginFinish_CreateLobbyUI: AEvent<EventType.LoginFinish> {

        protected override async ETTask Run(Scene scene, EventType.LoginFinish args) {
            await UIHelper.Create(scene, UIType.UILobby, UILayer.Mid);
        }
    }
=======
﻿namespace ET.Client
{
	[Event(SceneType.Client)]
	public class LoginFinish_CreateLobbyUI: AEvent<Scene, EventType.LoginFinish>
	{
		protected override async ETTask Run(Scene scene, EventType.LoginFinish args)
		{
			await UIHelper.Create(scene, UIType.UILobby, UILayer.Mid);
		}
	}
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
}
