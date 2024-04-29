using System;
namespace ET.Client {
    public static class EnterMapHelper {
        public static async ETTask EnterMapAsync(Scene clientScene) {
            try {
				// 【客户端】发消息给【网关服】：说客户端想要进地图。。
                G2C_EnterMap g2CEnterMap = await clientScene.GetComponent<SessionComponent>().Session.Call(new C2G_EnterMap()) as G2C_EnterMap;
                clientScene.GetComponent<PlayerComponent>().MyId = g2CEnterMap.MyId; // 【地图服】给【客户端】分配了一个身份证 MyId
                
                // 等待场景切换完成：
                await clientScene.GetComponent<ObjectWait>().Wait<Wait_SceneChangeFinish>();
                
                EventSystem.Instance.Publish(clientScene, new EventType.EnterMapFinish());
            }
            catch (Exception e) {
                Log.Error(e);
            }    
        }
    }
}