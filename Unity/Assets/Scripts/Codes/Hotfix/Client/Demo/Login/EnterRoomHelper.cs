using System;
namespace ET.Client {

    // 如果每个按钮的回调：都单独一个类，不成了海量回调类了？

    // public static class EnterMapHelper {
    public static class EnterRoomHelper {

        // 进拖拉拉机房：异步过程，需要与房间服交互的. 【房间服】：
        // 【C2G_EnterRoom】：消息也改下。参考游戏里没有改，【去找】参考游戏如何进入游戏房间的？
        public static async ETTask EnterRoomAsync(Scene clientScene) {
            try {
                // 【这里参考得不对，逻辑不对】：不是进入地图，一定是进入房间
                // G2C_EnterMap g2CEnterMap = await clientScene.GetComponent<SessionComponent>().Session.Call(new C2G_EnterMap()) as G2C_EnterMap;
                // clientScene.GetComponent<PlayerComponent>().MyId = g2CEnterMap.MyId;
                
                // 等待场景切换完成
                await clientScene.GetComponent<ObjectWait>().Wait<Wait_SceneChangeFinish>();

                // 【再检查一下】：这里我是进入房间吗？要跟参考游戏对比一下
                // EventSystem.Instance.Publish(clientScene, new EventType.EnterMapFinish());
                EventSystem.Instance.Publish(clientScene, new EventType.EnterRoomFinish());
// 这个，再去找下，谁在订阅这个事件，如何带动游戏开启的状态？现重构游戏里，还不曾订阅什么对此事感兴趣的回调。
            }
            catch (Exception e) {
                Log.Error(e);
            }    
        }
    }
}