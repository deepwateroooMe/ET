namespace ET
{
	// 分发数值监听: 现在没有全服了，不知道要怎么改这里，还是直接简单粗暴把这个类删除掉
	[Event(SceneType.All)]  // 服务端Map需要分发, 客户端CurrentScene也要分发
	public class NumericChangeEvent_NotifyWatcher: AEvent<Scene, EventType.NumbericChange>
	{
		protected override async ETTask Run(Scene scene, EventType.NumbericChange args)
		{
			NumericWatcherComponent.Instance.Run(args.Unit, args);
			await ETTask.CompletedTask;
		}
	}
}
