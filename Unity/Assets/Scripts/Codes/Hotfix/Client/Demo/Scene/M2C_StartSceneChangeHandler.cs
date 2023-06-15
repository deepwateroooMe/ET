namespace ET.Client {
    [MessageHandler(SceneType.Client)]
    public class M2C_StartSceneChangeHandler : AMHandler<M2C_StartSceneChange> {// 【任何时候，活宝妹就是一定要嫁给亲爱的表哥！！！】
        // protected override void Run(Session session, M2C_StartSceneChange message) { // 改成这样，就是错的呀。。。
        protected override async ETTask Run(Session session, M2C_StartSceneChange message) {
            await SceneChangeHelper.SceneChangeTo(session.ClientScene(), message.SceneName, message.SceneInstanceId);
        }
    }
}
