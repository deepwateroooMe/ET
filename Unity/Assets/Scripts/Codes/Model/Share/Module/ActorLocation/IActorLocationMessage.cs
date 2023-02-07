namespace ET {

// 不知道下面的这两个: 用法上的 主要区别 是什么 ?

    // 由于它是一条需要将经过Gate网关进行转发的一条消息，所以我们必须加上RpcId
    public interface IActorLocationMessage: IActorRequest {}

    // 这是一条由Client经过Gate网关进行转发到Map的Unit对象进行消息的处理和回复,感觉这说得不对呀  ？    
    public interface IActorLocationRequest: IActorRequest {} 

    public interface IActorLocationResponse: IActorResponse {}
}