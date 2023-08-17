namespace ET {
    // 那么，这里最直接的改法就是：添加一个不带参数的构造器
    public class MessageHandlerAttribute: BaseAttribute {
        public SceneType SceneType { get; }

        public MessageHandlerAttribute() { // 这个构造器：自己加的，骗过编译器。。
        }
        public MessageHandlerAttribute(SceneType sceneType) {
            this.SceneType = sceneType;
        }
    }
}