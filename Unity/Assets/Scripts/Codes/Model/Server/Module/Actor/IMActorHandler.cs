using System;
namespace ET.Server {
    public interface IMActorHandler {
        // 下面，参考的是ET-EUI 可能是 6.0 版本。ET7 里，可能接口还可以简化，还是Actor 消息机制模块简化了，不一定如下面这样
        // 下面，试着把这个方法改成，适配一个具体实现类的接口定义形式
        void Handle(Entity entity, int fromProcess, object actorMessage); // 不知道这里为什么需要再多一个的接口，我把返回重改回了 ETTask
        // ETTask Handle(Entity entity, int fromProcess, object actorMessage);
        // ETTask Handle(Entity entity, object actorMessage, Action<IActorResponse> reply);
        Type GetRequestType();
        Type GetResponseType();
    }
}
