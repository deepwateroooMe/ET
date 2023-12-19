namespace ET {
	// 只有三种状态：常年在 Pending, 除非 Faulted, 完成是 Succeeded. 方便【异步任务的状态监测】
    public enum AwaiterStatus: byte {
        // The operation has not yet completed.
        Pending = 0,
        // The operation completed successfully.
        Succeeded = 1,
        // The operation completed with an error.
        Faulted = 2,
    }
}