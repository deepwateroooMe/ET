namespace ET.Server {
    // 【没弄明白：】为什么现框架，把它精减到这么小了，只带一个变量。也就是说，只有这一个变量是必要的
    // 这里，我不能再照模仿原参考项目的，因为新的封装里，把消息的发送等封装到了底层。若我要拿 ActorMessageSender, 我是需要去 ActorMessageSenderComponent 组件里去拿的，不必加方法？
    // [ComponentOf(typeof(Unit))]
    [ComponentOf(typeof(Gamer))]
    public class UnitGateComponent : Entity, IAwake<long>, ITransfer {

        public long GateSessionActorId { get; set; }

        // // 感觉下面这个方法：不再必要，也不应该，也会报错的
		// public ActorMessageSender GetActorMessageSender() {
		// 	return Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(this.GateSessionActorId);
		// }
    }
}