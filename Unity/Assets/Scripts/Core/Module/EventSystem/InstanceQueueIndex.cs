namespace ET {
    // Unity 生命周期回调函数：在框架里有定义、可实现或更新的接口函数类型
    public enum InstanceQueueIndex {
        None = -1,
        Start, // 需要把这个回调加入框架统筹管理里去。Awake() 添加组件的时候，框架底层封装为会自动调用一次；同样Destroy() 回收，封装在框架底层
        Update,
        LateUpdate,
        Load, // 这个，算是框架的特色封装
        Max,
    }
}