namespace ET.Client {

    [MessageHandler(SceneType.Client)]
    public class M2C_PathfindingResultHandler : AMHandler<M2C_PathfindingResult> {

        protected override async ETTask Run(Session session, M2C_PathfindingResult message) { // 这些都是先前自己不懂的时候，瞎改的结果。。。。
            Unit unit = session.DomainScene().CurrentScene().GetComponent<UnitComponent>().Get(message.Id);
            float speed = unit.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);
            await unit.GetComponent<MoveComponent>().MoveToAsync(message.Points, speed);
        }
    }
}
