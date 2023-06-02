namespace ET.Server {
    // 离开视野: 大概是说，场景里的某个小人走到视野里看不见的地方
    [Event(SceneType.Map)]
    public class UnitLeaveSightRange_NotifyClient: AEvent<EventType.UnitLeaveSightRange> {
        protected override async ETTask Run(Scene scene, EventType.UnitLeaveSightRange args) {
            await ETTask.CompletedTask;
            AOIEntity a = args.A;
            AOIEntity b = args.B;
            if (a.Unit.Type != UnitType.Player) {
                return;
            }
            MessageHelper.NoticeUnitRemove(a.GetParent<Unit>(), b.GetParent<Unit>());
        }
    }
}