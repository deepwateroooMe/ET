using System;
namespace ET.Server {

    public interface IMActorHandler {
        // ETTask Handle(Entity entity, int fromProcess, object actorMessage);
        void Handle(Entity entity, int fromProcess, object actorMessage); // 自已改成这样的
        Type GetRequestType();
        Type GetResponseType();
    }
}