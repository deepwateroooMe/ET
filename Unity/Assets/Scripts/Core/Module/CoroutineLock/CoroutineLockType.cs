namespace ET {
    // 【下面，各种强调】：不用协程锁的使用上下文场景的类型，这个【使用上下文场景类型】，定义的究竟是什么，不同类型之间，是如何区分的？
// 分别对应以下使用情景：
    // 多个向location查询同一个实体真实进程号地址（在访问跨进程实体时）【这里是，被查询的实体的真实进程号地址，信息会被锁，被查询者！】，访问一次获得进程地址即可 ；
    // 【多个针对同一个实体对象，发起的Actor消息】；这里是【发送Actor 消息的实体】，同一个实体，发送消息是并发，一次只能发一个消息。加锁是因为，单线程多进程框架里，多个进程想要消息入队这个可【发送消息的 actorid】，对ActorLocationSender 队列上锁？
    // 多个处理针对同一个实体的Mailbox消息处理，处理需要按照先后顺序；
    // 针对Map中单位上下线时，新上线消息需要等待下线消息执行完后再处理；（自己先前接触过的，玩家客户端某端下线、某端上线、或自顶号操作，保证执行先后顺序的锁，锁住针对的是，一个某定玩家 UnitId）
    // 针对同一个DB访问的前后顺序处理；
    // 多个资源请求同一个ab包下载的处理。
    // 【可添加自定义类型：】如果还有自己想要处理的协程锁类型，可自行添加，不过现在这些应该已经够用了，且ET6.0中猫大大部分都已经封装处理好了，不需要我们再写了。
    public static class CoroutineLockType { // 对应的，就是框架里，线程安全、资源安全，几处使用到锁的上下文场景
        public const int None = 0;
        public const int Location = 1;                  // location进程上使用
        public const int ActorLocationSender = 2;       // ActorLocationSender中队列消息。【重点看这个】: 这个【A发请求拿B 位置的消息】，发起者A, 与【被请求者B】, 锁的是发问者A
        public const int Mailbox = 3;                   // Mailbox中队列
        public const int UnitId = 4;                    // Map服务器上线下线时使用
        public const int DB = 5;
        public const int Resources = 6;
        public const int ResourcesLoader = 7;
        public const int Max = 100; // 这个必须最大
    }
}
