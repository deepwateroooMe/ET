namespace ET
{
    // 标记异步任何的当前状态:等待， 成功完成，  失败
    public enum AwaiterStatus: byte
    {
        /// <summary>The operation has not yet completed.</summary>
        Pending = 0,

        /// <summary>The operation completed successfully.</summary>
        Succeeded = 1,

        /// <summary>The operation completed with an error.</summary>
        Faulted = 2,
    }
}