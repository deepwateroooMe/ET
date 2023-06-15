namespace ET.Server {
    [ActorMessageHandler(SceneType.Map)]
    public class C2M_StopHandler : AMActorLocationHandler<Unit, C2M_Stop> {
        // protected override async ETTask Run(Unit unit, C2M_Stop message) {
        protected override async ETTask Run(Unit unit, C2M_Stop message) {
            unit.Stop(1);
            // 应该需要去把 ETVoid 弄明白，为什么不能 access ？
           await ETTask.CompletedTask;
        }
    }
}