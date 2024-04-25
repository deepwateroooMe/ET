namespace ET {
	// ET 框架的事件系统：会对框架里的 entity 的 InstanceId 分门别类，底层各种、种种、层层封装、以便进行系统化优化与管理。
	// 这里，分门别类——Unity 里生命周期函数的回调调用与触发，显得重要
    public static class InstanceQueueIndex {
        public const int None = -1;

		// Start() 回调，可以添加这里吗？可能Start() 不是【必须、不得不要】的？能否减掉？重构项目里的【TODO】：翻框架，细节确认！
		// Start() 好像框架里、哪里的？处理逻辑是；在第一次Update() 每桢调用之前，仅只调用一次Start();
		// 以后每桢，只调用每桢的回调：Update() LateUpdate()-etc 每桢的生命周期回调函数
        public const int Update = 0; // Awake() 但凡添加组件自动调用一次，没算；
        public const int LateUpdate = 1;

        public const int Load = 2;
        public const int Max = 3; // 最多只能 3 ？【TODO】：
    }
}