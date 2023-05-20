using System;
using ET;
using System.Threading.Tasks;
namespace ET.Server {
    // 匹配服务器：处理网关网关服发送的匹配玩家请求【这里就没有找到，这个匹配服务器什么时候添加的内网组件？】
    [MessageHandler(SceneType.Match)]
    public class G2M_PlayerEnterMatch_ReqHandler : AMRpcHandler<G2M_PlayerEnterMatch_Req, M2G_PlayerEnterMatch_Ack> {

        protected override async void Run(Session session, G2M_PlayerEnterMatch_Req message, Action<M2G_PlayerEnterMatch_Ack> reply) {
            M2G_PlayerEnterMatch_Ack response = new M2G_PlayerEnterMatch_Ack();
            try {
                MatchComponent matchComponent = Root.Instance.Scene.GetComponent<MatchComponent>(); // Program.cs 全局添加的
                ActorMessageSenderComponent actorProxyComponent = Root.Instance.Scene.GetComponent<ActorMessageSenderComponent>();
                if (matchComponent.Playing.ContainsKey(message.UserID)) { // 如果 match 过，连接过程中失败了，再给它重新连接一下：重连就是，再发一次进特定房间消息申请
                    MatchRoomComponent matchRoomComponent = Root.Instance.Scene.GetComponent<MatchRoomComponent>();
                    long roomId = matchComponent.Playing[message.UserID]; // 这个长整型：带着很多信息，可以获取到 actorID
                    Room room = matchRoomComponent.Get(roomId);
                    Gamer gamer = room.Get(message.UserID);
                    // 重置GateActorID
                    gamer.PlayerID = message.PlayerID;
                    // 重连房间
                    ActorMessageSender actorProxy = actorProxyComponent.Get(roomId); // 拿到一个发ActorMessage 的包装 ActorMessageSender
                    await actorProxy.Call(new Actor_PlayerEnterRoom_Req() {
                            PlayerID = message.PlayerID,
                                UserID = message.UserID,
                                SessionID = message.SessionID
                                });
                    // 向玩家发送匹配成功消息
                    ActorMessageSender gamerActorProxy = actorProxyComponent.Get(gamer.PlayerID);
                    gamerActorProxy.Send(new Actor_MatchSucess_Ntt() { GamerID = gamer.Id });
                }
                else { // 不曾分配，去分配
                    // 创建匹配玩家
                    Matcher matcher = MatcherFactory.Create(message.PlayerID, message.UserID, message.SessionID);
                }
                reply(response);
            }
            catch (Exception e) {
                ReplyError(response, e, reply);
            }
        }
    }
}
