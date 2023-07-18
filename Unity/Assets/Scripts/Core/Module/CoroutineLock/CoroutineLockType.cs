namespace ET {
    public static class CoroutineLockType { // 对应的，就是框架里，线程安全、资源安全，几处使用到锁的上下文场景
        public const int None = 0;
        public const int Location = 1;                  // location进程上使用
        public const int ActorLocationSender = 2;       // ActorLocationSender中队列消息。【重点看这个】
        public const int Mailbox = 3;                   // Mailbox中队列
        public const int UnitId = 4;                    // Map服务器上线下线时使用
        public const int DB = 5;
        public const int Resources = 6;
        public const int ResourcesLoader = 7;
        public const int Max = 100; // 这个必须最大
    }
}