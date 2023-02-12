namespace ET {

    // 场景类型: 这里 场景,不要理解为unity游戏里的场景,理解为服务器或是客户端任何需要事件回调调用时的 上下文场景
    // 然后,这里面就有狠多场景,自己就搞不明白说的到底是什么意思,或者说什么情况下会需要使用哪种类型的场景
    public enum SceneType {
        None = -1,
        Process = 0,
        Manager = 1,
        Realm = 2, //  注册登录服
        Gate = 3,  // 网关服
        Http = 4,  // 文件管理器吗？还是说异步网络调用的封装 ？
        Location = 5, // 定义服，
        Map = 6,   // 地图 ？
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