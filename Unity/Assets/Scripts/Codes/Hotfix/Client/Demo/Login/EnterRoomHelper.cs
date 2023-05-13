using System;
namespace ET.Client {

    // 如果每个按钮的回调：都单独一个类，不成了海量回调类了？

    // public static class EnterMapHelper {
    public static class EnterRoomHelper {

        // 进拖拉拉机房：异步过程，需要与房间服交互的. 【房间服】：
        // 【C2G_EnterRoom】：消息也改下
        public static async ETTask EnterRoomAsync(Scene clientScene) {
            try {
                G2C_EnterMap g2CEnterMap = await clientScene.GetComponent<SessionComponent>().Session.Call(new C2G_EnterMap()) as G2C_EnterMap;
                clientScene.GetComponent<PlayerComponent>().MyId = g2CEnterMap.MyId;
                
                // 等待场景切换完成
                await clientScene.GetComponent<ObjectWait>().Wait<Wait_SceneChangeFinish>();
                
                // EventSystem.Instance.Publish(clientScene, new EventType.EnterMapFinish());
                EventSystem.Instance.Publish(clientScene, new EventType.EnterRoomFinish()); // 这个，再去找下，谁在订阅这个事件，如何带动游戏开启的状态？
                
                // // 老版本：斗地主里，进入地图的参考【ET7】里，就要去找，如何处理这些组件的？
                // Game.Scene.AddComponent<OperaComponent>();
                // Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILobby);
            }
            catch (Exception e) {
                Log.Error(e);
            }    
        }
    }
}