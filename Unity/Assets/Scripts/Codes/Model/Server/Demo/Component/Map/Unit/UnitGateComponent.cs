namespace ET.Server {
    // 【没弄明白：】为什么现框架，把它精减到这么小了，只带一个变量。也就是说，只有这一个变量是必要的
    // 这里，我不能再照模仿原参考项目的，因为新的封装里，把消息的发送等封装到了底层。若我要拿 ActorMessageSender, 我是需要去 ActorMessageSenderComponent 组件里去拿的，不必加方法？

    // [ComponentOf(typeof(Gamer))]
    // [ComponentOf(typeof(User))]  // 这里为什么会成为：同一个组件只能为一个什么XX 的子组件组成部分？
    // [ComponentOf(typeof(Unit))]
    [ComponentOf]  // <<<<<<<<<<<<<<<<<<<< 这里标记了三个，就不要再标记（ X)）就可以了！！！下午家里会测试一下
        public class UnitGateComponent : Entity, IAwake<long>, ITransfer, ISerializeToEntity { // 不知道这里为什么会受到限制，这里再改一下
        public long GateSessionActorId { get; set; }
        // 想一下，下面的变更还需要吗？要不要，是看框架里有没有什么，自动上线自动下线处理之类的，相关的？
        public bool IsDisconnect;
        
        // // 感觉下面这个方法：不再必要，也不应该，也会报错的, 因为框架重构了，不再一个一个地发消息（这里是说跨进程消息？）
		// public ActorMessageSender GetActorMessageSender() {
		// 	return Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(this.GateSessionActorId);
		// }
    }
}
