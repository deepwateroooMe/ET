using System;
namespace ET.Server {
    public interface IMActorHandler { // 忘记这里IM 标记什么了，框架里的某种标记
        ETTask Handle(Entity entity, int fromProcess, object actorMessage);
        Type GetRequestType();
        Type GetResponseType();
    }
}