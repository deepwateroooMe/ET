using System;
using System.Net;
using ET;
namespace ET.Server {
    [MessageHandler(SceneType.Gate)]
    public class R2G_PlayerKickOut_ReqHandler : AMRpcHandler<R2G_PlayerKickOut_Req, G2R_PlayerKickOut_Ack> {
        // protected override async void Run(Session session, R2G_PlayerKickOut_Req message, Action<G2R_PlayerKickOut_Ack> reply) {
        protected override async ETTask Run(Session session, R2G_PlayerKickOut_Req message, G2R_PlayerKickOut_Ack response) {
            User user = session.DomainScene().GetComponent<UserComponent>().Get(message.UserID);
            // 服务端主动断开客户端连接
            long userSessionId = user.GetComponent<UnitGateComponent>().GateSessionActorId;
            await session.DomainScene().GetComponent<NetOuterComponent>().Remove(userSessionId); // 没有这个组件了，换别的
            Log.Info($"将玩家{message.UserID}连接断开");
        }
    }
}