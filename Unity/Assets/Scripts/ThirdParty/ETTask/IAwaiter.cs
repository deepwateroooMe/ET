namespace ET {
    public enum AwaiterStatus: byte {
        // The operation has not yet completed.
        Pending = 0,
        // The operation completed successfully.
        Succeeded = 1,
        // The operation completed with an error.
        Faulted = 2,
    }
}