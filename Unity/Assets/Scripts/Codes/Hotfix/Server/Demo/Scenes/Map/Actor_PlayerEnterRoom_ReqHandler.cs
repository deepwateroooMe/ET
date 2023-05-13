using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ETModel;
namespace ET.Server {

    // 【房间服】：匹配好了的玩家，请求进入房间
    [ActorMessageHandler(AppType.Map)]
    public class Actor_PlayerEnterRoom_ReqHandler : AMActorRpcHandler<Room, Actor_PlayerEnterRoom_Req, Actor_PlayerEnterRoom_Ack> {

        protected override async Task Run(Room room, Actor_PlayerEnterRoom_Req message, Action<Actor_PlayerEnterRoom_Ack> reply) {
            Actor_PlayerEnterRoom_Ack response = new Actor_PlayerEnterRoom_Ack();
            try {
                Gamer gamer = room.Get(message.UserID);
                if (gamer == null) { // 当前玩家，在这个被分配的房间里，还没被初始化
                    // 创建房间玩家对象
                    gamer = GamerFactory.Create(message.PlayerID, message.UserID);
                    await gamer.AddComponent<MailBoxComponent>().AddLocation(); // 只有给玩家挂上这个组件，并向中央邮件注册登记地址，接下来的游戏它才可以收发消息，出牌什么的
                    gamer.AddComponent<UnitGateComponent, long>(message.SessionID);
                    // 加入到房间
                    room.Add(gamer); // 这里就又多一步逻辑处理：这里当服务器匹配成功一个玩家，就去做相应的客户端视图层相应的变动调动
                    Actor_GamerEnterRoom_Ntt broadcastMessage = new Actor_GamerEnterRoom_Ntt();
                    foreach (Gamer _gamer in room.GetAll()) {
                        if (_gamer == null) {
                            // 添加空位
                            broadcastMessage.Gamers.Add(new GamerInfo());
                            continue;
                        }
                        // 添加玩家信息
                        GamerInfo info = new GamerInfo() { UserID = _gamer.UserID, IsReady = _gamer.IsReady };
                        broadcastMessage.Gamers.Add(info);
                    }
                    // 广播房间内玩家消息：为什么这里说是广播消息？
                    room.Broadcast(broadcastMessage);
                    Log.Info($"玩家{message.UserID}进入房间");
                } else {  // 这里大致把这部分的逻辑看一下，要去弄好吃了。。。。。【这里没看完，下次接着看。爱表哥，爱生活！！！】
                    // 玩家重连
                    gamer.isOffline = false;
                    gamer.PlayerID = message.PlayerID;
                    gamer.GetComponent<UnitGateComponent>().GateSessionActorId = message.SessionID;
                    // 玩家重连，移除托管组件
                    gamer.RemoveComponent<TrusteeshipComponent>();
                    Actor_GamerEnterRoom_Ntt broadcastMessage = new Actor_GamerEnterRoom_Ntt();
                    foreach (Gamer _gamer in room.GetAll()) {
                        if (_gamer == null) {
                            // 添加空位
                            broadcastMessage.Gamers.Add(default(GamerInfo));
                            continue;
                        }
                        // 添加玩家信息
                        GamerInfo info = new GamerInfo() { UserID = _gamer.UserID, IsReady = _gamer.IsReady };
                        broadcastMessage.Gamers.Add(info);
                    }
                    // 发送房间玩家信息
                    ActorMessageSender actorProxy = gamer.GetComponent<UnitGateComponent>().GetActorMessageSender();
                    actorProxy.Send(broadcastMessage);
                    List<GamerCardNum> gamersCardNum = new List<GamerCardNum>();
                    List<GamerState> gamersState = new List<GamerState>();
                    GameControllerComponent gameController = room.GetComponent<GameControllerComponent>();
                    OrderControllerComponent orderController = room.GetComponent<OrderControllerComponent>();
                    DeskCardsCacheComponent deskCardsCache = room.GetComponent<DeskCardsCacheComponent>();
                    foreach (Gamer _gamer in room.GetAll()) {
                        HandCardsComponent handCards = _gamer.GetComponent<HandCardsComponent>();
                        gamersCardNum.Add(new GamerCardNum() {
                                UserID = _gamer.UserID,
                                    Num = _gamer.GetComponent<HandCardsComponent>().GetAll().Length
                                    });
                        GamerState gamerState = new GamerState() {
                            UserID = _gamer.UserID,
                            UserIdentity = handCards.AccessIdentity
                        };
                        if (orderController.GamerLandlordState.TryGetValue(_gamer.UserID, out bool state)) {
                            if (state) {
                                gamerState.State = GrabLandlordState.Grab;
                            }
                            else {
                                gamerState.State = GrabLandlordState.UnGrab;
                            }
                        }
                        gamersState.Add(gamerState);
                    }
                    // 发送游戏开始消息
                    Actor_GameStart_Ntt gameStartNotice = new Actor_GameStart_Ntt();
                    gameStartNotice.HandCards.AddRange(gamer.GetComponent<HandCardsComponent>().GetAll());
                    gameStartNotice.GamersCardNum.AddRange(gamersCardNum);
                    actorProxy.Send(gameStartNotice);
                    Card[] lordCards = null;
                    if (gamer.GetComponent<HandCardsComponent>().AccessIdentity == Identity.None) {
                        // 广播先手玩家
                        actorProxy.Send(new Actor_AuthorityGrabLandlord_Ntt() { UserID = orderController.CurrentAuthority });
                    } else {
                        if (gamer.UserID == orderController.CurrentAuthority) {
                            // 发送可以出牌消息
                            bool isFirst = gamer.UserID == orderController.Biggest;
                            actorProxy.Send(new Actor_AuthorityPlayCard_Ntt() { UserID = orderController.CurrentAuthority, IsFirst = isFirst });
                        }
                        lordCards = deskCardsCache.LordCards.ToArray();
                    }
                    // 发送重连数据
                    Actor_GamerReconnect_Ntt reconnectNotice = new Actor_GamerReconnect_Ntt() {
                        UserId = orderController.Biggest,
                        Multiples = room.GetComponent<GameControllerComponent>().Multiples
                    };
                    reconnectNotice.GamersState.AddRange(gamersState);
                    reconnectNotice.Cards.AddRange(deskCardsCache.library);
                    if (lordCards != null) {
                        reconnectNotice.LordCards.AddRange(lordCards);
                    }
                    actorProxy.Send(reconnectNotice);
                    Log.Info($"玩家{message.UserID}重连");
                }
                response.GamerID = gamer.InstanceId;
                reply(response);
            }
            catch (Exception e) {
                ReplyError(response, e, reply);
            }
        }
    }
}
