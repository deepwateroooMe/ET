using System;
namespace ET.Server {
    public interface IMActorHandler { // 忘记这里IM 标记什么了，框架里的某种标记: 就当是 Actor Message Interface 处理器，倒过来的：IMActorHandler 
        ETTask Handle(Entity entity, int fromProcess, object actorMessage);
        Type GetRequestType();
        Type GetResponseType();
    }
}