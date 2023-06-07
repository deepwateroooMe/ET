using System;
namespace ET {
    public interface IMHandler {
        // 下面，返回类型不对
        void Handle(Session session, object message); // 这里返回类型，仍然应该是ETTask, 或者可能的 ETVoid ？

        // 消息处理器帮助类，在程序域加载的时候，会自动扫描程序域里的ActorMessageHander 标签，会想要拿消息的【发送类型】与消息的【返回类型】，来系统化管理消息处理 
        Type GetMessageType();
        Type GetResponseType(); // 不应该把这个去掉。
    }
}