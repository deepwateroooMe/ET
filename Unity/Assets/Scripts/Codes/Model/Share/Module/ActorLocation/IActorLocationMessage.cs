namespace ET {

	// 以为它是普通位置消息IMessage 或 IActorMessage, 但它实际是 IActorRequest/IRequest 消息
	// 位置 Location 相关的：永远需要，回复消息。也可以理解：因为这类消息，一般是想要：查询某实例的位置信息，需要返回答案
	// IActorMessage 实例，位置不相关的，是存在，不需要回复消息的。框架里这类实例，细看一下处理方式
    public interface IActorLocationMessage: IActorLocationRequest {}

	public interface IActorLocationRequest: IActorRequest {}
    public interface IActorLocationResponse: IActorResponse {}
}