using ET;
using System;
using System.Threading.Tasks;
namespace ET {
    public interface IMActorHandler {
        // ETTask Handle(Entity entity, object actorMessage, Action<IActorResponse> reply);
        void Handle(Entity entity, object actorMessage, Action<IActorResponse> reply); // 我自己把这里又改掉了 
        Type GetRequestType();
        Type GetResponseType();
    }
    // public interface IMActorHandler {
    //     Task Handle(Session session, Entity entity, object actorMessage);
    //     Type GetMessageType();
    // }
}