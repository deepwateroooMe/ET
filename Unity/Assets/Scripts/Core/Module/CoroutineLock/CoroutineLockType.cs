namespace ET {

    // 根据 可能会使用到协程 的不同 情境上下文 进行分组标记: 以便接下来可以区分处理
    public static class CoroutineLockType {

        public const int None = 0;
        public const int Location = 1;                  // location进程上使用
        public const int ActorLocationSender = 2;       // ActorLocationSender 中队列消息 
        public const int Mailbox = 3;                   // Mailbox中队列
        public const int UnitId = 4;                    // Map服务器上线下线时使用
        public const int DB = 5;
        public const int Resources = 6;
        public const int ResourcesLoader = 7;
        public const int Max = 100; // 这个必须最大
    }
}