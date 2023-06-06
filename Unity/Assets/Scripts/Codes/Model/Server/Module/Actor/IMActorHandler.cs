using System;
namespace ET.Server {

    public interface IMActorHandler {
        // 下面，参考的是ET-EUI 可能是 6.0 版本。ET7 里，可能接口还可以简化，还是Actor 消息机制模块简化了，不一定如下面这样
        ETTask Handle(Entity entity, object actorMessage, Action<IActorResponse> reply);
        Type GetRequestType();
        Type GetResponseType();
    }
}
