namespace ET
{
	// 场景类型: 这里 场景,不要理解为unity游戏里的场景,理解为服务器或是客户端任何需要事件回调调用时的 上下文场景
	public enum SceneType
	{
		None = -1,
		Process = 0,
		Manager = 1,
		Realm = 2,
		Gate = 3,
		Http = 4,
		Location = 5,
		Map = 6,
		Router = 7,
		RouterManager = 8,
		Robot = 9,
		BenchmarkClient = 10,
		BenchmarkServer = 11,
		Benchmark = 12,

		// 客户端Model层
		Client = 31,
		Current = 34,
	}
}