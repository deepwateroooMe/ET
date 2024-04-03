namespace ET {
    public static class InstanceQueueIndex {
        public const int None = -1;

		// Start() 回调，可以添加这里吗？可能Start() 不是【必须、不得不要】的？能否减掉？重构项目里的
        public const int Update = 0; // Awake() 但凡添加组件自动调用一次，没算；
        public const int LateUpdate = 1;

        public const int Load = 2;
        public const int Max = 3; // 最多只能 3 ？【TODO】：
    }
}