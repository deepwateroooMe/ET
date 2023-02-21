using System;
using System.Net;
namespace ET.Server {
// 注册登录服 Realm 处理客户端 Login 的定义逻辑：它向网关服为客户端请求一个 key, 以便接下来客户端与网关服建立一个会话框，以后这个客户端就是网关服直接通信

    // 标记为这个的方法，会由MessageDispatcherComponent分发处理，即由当前进程服务器的消息派发器直接处理（也表示这个处理的协议为普通的协议）
    [MessageHandler(SceneType.Realm)] // 打标签：会被系统性的扫描注册处理器，并在扫描启动时自动生成处理器实例
    public class C2R_LoginHandler : AMRpcHandler<C2R_Login, R2C_Login> {
        // 派生于AMRpcHandler<C2R_Login, R2C_Login>，表示接受一个C2R_Login类型协议，返回一个R2C_Login协议，AMRpcHandler类封装了回复消息的统一处理，即传入的Action reply

        // 获取一个随机的Gate的相关配置，Realm向Gate服务发送一条Actor请求，拿到一个客户端登录Gate用的认证Key。这里姑且当直接拿到一个认证key，并且填到回复类实例中，调用reply将回复协议发送回给客户端
        protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response) {
            // 随机分配一个Gate
            StartSceneConfig config = RealmGateAddressHelper.GetGate(session.DomainZone());
            Log.Debug($"gate address: {MongoHelper.ToJson(config)}");
            
            // 向gate请求一个key,客户端可以拿着这个key连接gate
            G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await ActorMessageSenderComponent.Instance.Call(
                config.InstanceId, new R2G_GetLoginKey() {Account = request.Account});
            response.Address = config.InnerIPOutPort.ToString();
            response.Key = g2RGetLoginKey.Key;
            response.GateId = g2RGetLoginKey.GateId;
        }
    }
}
