namespace ET {
    public enum InstanceQueueIndex {
        None = -1,
        Start, // 需要把这个回调加入框架统筹管理里去 
        Update,
        LateUpdate,
        Load,
        Max,
    }
}