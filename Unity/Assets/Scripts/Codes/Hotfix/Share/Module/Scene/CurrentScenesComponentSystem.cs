using System;
namespace ET {
	// 它没有生命，所以是静态类、帮助类，只是帮助去返回场景，与【生命周期】无关
    public static class CurrentScenesComponentSystem {
        public static Scene CurrentScene(this Scene clientScene) {
            return clientScene.GetComponent<CurrentScenesComponent>()?.Scene;
        }
    }
}