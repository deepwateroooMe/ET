namespace ET {

	// 以为它是普通位置消息IMessage 或 IActorMessage, 但它实际是 IActorRequest/IRequest 消息
	// 位置 Location 相关的：永远需要，回复消息
	// IActorMessage 实例，位置不相关的，是存在，不需要回复消息的

	// 感觉最特殊的是子接口、继承接口：IActorLocationMessage, 居然是继承 IActorRequest.
	// 这使得所有的位置相关Message 全带返回消息，保障消息发送成功，因为查询位置的消息框架应该比较少用。鲜有用到、又要用到、就要保障发送成功
    public interface IActorLocationMessage: IActorLocationRequest {}

	public interface IActorLocationRequest: IActorRequest {}
    public interface IActorLocationResponse: IActorResponse {}
}