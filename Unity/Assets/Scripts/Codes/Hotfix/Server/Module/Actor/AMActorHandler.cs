using System;
using System.Threading.Tasks;

namespace ET.Server {
    [ActorMessageHandler(SceneType.Gate)] // 不知道是哪个服在处理这个ActorMessage, 暂时先写成是网关服。再检查一下
    public abstract class AMActorHandler<E, Message>: IMActorHandler where E : Entity where Message : class, IActorMessage {
        // 把下面这个抽象方法的返回类型，改了
        protected abstract void Run(E entity, Message message);
        // protected abstract ETTask Run(E entity, Message message);

        public void Handle(Entity entity, int fromProcess, object actorMessage) {
            Message msg = actorMessage as Message;
            if (msg == null) {
                Log.Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Message).Name}");
                return;
            }
            E e = entity as E;
            if (e == null) {
                Log.Error($"Actor类型转换错误: {entity.GetType().Name} to {typeof (E).Name} --{typeof (Message).Name}");
                return;
            }
            this.Run(e, msg); // 当把这个方法不再写成异步方法，就直接运行就可以了？【到时检查运行时错误】
            // await this.Run(e, msg);
        }
        public Type GetRequestType() {
            if (typeof (IActorLocationMessage).IsAssignableFrom(typeof (Message))) {
                Log.Error($"message is IActorLocationMessage but handler is AMActorHandler: {typeof (Message)}");
            }
            return typeof (Message);
        }
        public Type GetResponseType() {
            return null;
        }

        // 意思说，这里不同版本之间又不对应了？再去检查一下
	}
}
