using System;
using System.IO;
namespace ET.Client {
	// 【客户端】的启动逻辑：
    [Event(SceneType.Process)]
    public class EntryEvent3_InitClient: AEvent<Scene, ET.EventType.EntryEvent3> {

        protected override async ETTask Run(Scene scene, ET.EventType.EntryEvent3 args) {
            // 加载配置
            Root.Instance.Scene.AddComponent<ResourcesComponent>(); // 【资源包】相关模块，感觉狠熟悉了，不看
            Root.Instance.Scene.AddComponent<GlobalComponent>();
            await ResourcesComponent.Instance.LoadBundleAsync("unit.unity3d");
            Scene clientScene = await SceneFactory.CreateClientScene(1, "Game"); // 加载：【客户端】初始化游戏场景Game
            await EventSystem.Instance.PublishAsync(clientScene, new EventType.AppStartInitFinish());
        }
    }
}