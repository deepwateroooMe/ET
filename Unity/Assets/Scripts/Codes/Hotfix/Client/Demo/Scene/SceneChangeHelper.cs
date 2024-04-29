namespace ET.Client {
    public static class SceneChangeHelper {
        // 场景切换协程
		// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
		// 现在，算是，狠熟悉【客户端】游戏界面流程了！！【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
        public static async ETTask SceneChangeTo(Scene clientScene, string sceneName, long sceneInstanceId) {
            clientScene.RemoveComponent<AIComponent>(); // 不知道，什么时候添加了这狗屁玩意儿。。
            CurrentScenesComponent currentScenesComponent = clientScene.GetComponent<CurrentScenesComponent>();
            currentScenesComponent.Scene?.Dispose(); // 删除之前的CurrentScene，创建新的
            Scene currentScene = SceneFactory.CreateCurrentScene(sceneInstanceId, clientScene.Zone, sceneName, currentScenesComponent);
            UnitComponent unitComponent = currentScene.AddComponent<UnitComponent>();
         
            // 可以订阅这个事件中创建Loading界面
            EventSystem.Instance.Publish(clientScene, new EventType.SceneChangeStart());
            // 等待CreateMyUnit的消息：【协程】会停在这里，直接【客户端】接收到【地图服】发来 Wait_CreateMyUnit 已经完成、做完的消息
            Wait_CreateMyUnit waitCreateMyUnit = await clientScene.GetComponent<ObjectWait>().Wait<Wait_CreateMyUnit>();
            M2C_CreateMyUnit m2CCreateMyUnit = waitCreateMyUnit.Message;
            Unit unit = UnitFactory.Create(currentScene, m2CCreateMyUnit.Unit);
            unitComponent.Add(unit);
            clientScene.RemoveComponent<AIComponent>();
            EventSystem.Instance.Publish(currentScene, new EventType.SceneChangeFinish());

            // 通知等待场景切换的协程
            clientScene.GetComponent<ObjectWait>().Notify(new Wait_SceneChangeFinish());
        }
    }
}