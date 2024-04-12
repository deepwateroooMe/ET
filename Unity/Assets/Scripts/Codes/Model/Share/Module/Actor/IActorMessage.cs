namespace ET {

    // 不需要返回消息【源】：框架里有好多个用例。细看下它们的处理方式有什么不同【TODO】：IActorMessage 类型的！
    public interface IActorMessage: IMessage {}

    public interface IActorRequest: IRequest {}
    public interface IActorResponse: IResponse {}
}