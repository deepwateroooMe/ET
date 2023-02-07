namespace ET.Server {

    [ActorMessageHandler(SceneType.Map)]
    public class C2M_PathfindingResultHandler : AMActorLocationHandler<Unit, C2M_PathfindingResult> {

        // 大致是说:  你把这个玩家移到目标位置去,等待异步完成结束
        protected override async ETTask Run(Unit unit, C2M_PathfindingResult message) {
            unit.FindPathMoveToAsync(message.Position).Coroutine();
            await ETTask.CompletedTask;
        }
    }
}