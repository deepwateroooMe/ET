namespace ET.Client {
	// 【客户端场景】：接收到【地图服】转发来的【客户端开始切场景、命令】就开始切场景了
    [MessageHandler(SceneType.Client)]
    public class M2C_StartSceneChangeHandler : AMHandler<M2C_StartSceneChange> {

        protected override async ETTask Run(Session session, M2C_StartSceneChange message) {
            await SceneChangeHelper.SceneChangeTo(session.ClientScene(), message.SceneName, message.SceneInstanceId);
        }
    }
}
