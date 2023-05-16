using System;
namespace ET {
    public interface IMHandler {
        void Handle(Session session, object message);
        Type GetMessageType();
        //Type GetResponseType(); // 暂时把这个去掉：不知道是否会引发其它问题
    }
}