using ET;
using System;
using System.Threading.Tasks;
namespace ET {
    public interface IMActorHandler {
        Task Handle(Session session, Entity entity, object actorMessage);
        Type GetMessageType();
    }
}