namespace ET {
	// 规定了：几大不同类型【内网消息、外网消息】的【网络操作码】范围，用来区分和判断
    public static class OpcodeRangeDefine {
        public const ushort OuterMinOpcode = 10001;
        public const ushort OuterMaxOpcode = 20000;
        // 20001-30000 内网pb
        public const ushort InnerMinOpcode = 20001;
        public const ushort InnerMaxOpcode = 40000;
        public const ushort MaxOpcode = 60000;
    }
}