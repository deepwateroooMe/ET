namespace ET {

    // 不需要返回消息【源】：框架里有好多个用例。
	// 感觉最特殊的是子接口、继承接口：IActorLocationMessage, 居然是继承 IActorRequest.
	// 这使得所有的位置相关Message 全带返回消息，保障消息发送成功，因为查询位置的消息框架应该比较少用。鲜有用到、又要用到、就要保障发送成功
    public interface IActorMessage: IMessage {}

    public interface IActorRequest: IRequest {}
    public interface IActorResponse: IResponse {}
}