using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ET.Client {
    // 会被 UILobby 里三具按钮的点击都会触发调用
    [Event(SceneType.Client)]
    public class ModeSelected_RemoveLobbyUI: AEvent<EventType.ModeSelected> {
        protected override async ETTask Run(Scene scene, EventType.ModeSelected args) {
            await UIHelper.Remove(scene, UIType.UILobby);
        }
    }
}
