﻿namespace ET {
    // 这里的源码有点儿混乱了，不知乎算是怎么回事
    public enum SceneType {
        None = -1,
        Process = 0,  // 活宝妹脑袋里：一个核上的主线程特殊场景
        Manager = 1,
        Realm = 2,
        Gate = 3,
        Http = 4,
        Location = 5, // 位置服
        Map = 6,
        Router = 7,
        RouterManager = 8,
        Robot = 9,
        BenchmarkClient = 10,
        BenchmarkServer = 11,
        Benchmark = 12,
        Match = 13,  // 是可以加上 Match 服的
        AllServer = 14,  // 自已加的

        // 客户端Model层
        Client = 31,
        Current = 34,
    }
}