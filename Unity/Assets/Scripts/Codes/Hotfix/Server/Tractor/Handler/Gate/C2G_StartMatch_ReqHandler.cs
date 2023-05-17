﻿using System;
using ET;
using System.Net;
namespace ET.Server {

    // 网关服：处理客户端 StartMatch 请求消息
    [MessageHandler(SceneType.Gate)]
    public class C2G_StartMatch_ReqHandler : AMRpcHandler<C2G_StartMatch_Req, G2C_StartMatch_Ack> {

        // 现在处理的逻辑：几个步骤：会话框有效吗？从数据库提取玩家相关数据，玩家合乎标准吗？再转交匹配服去处理【暂时把数据库玩家验证跳过】
        protected override async void Run(Session session, C2G_StartMatch_Req message, Action<G2C_StartMatch_Ack> reply) {
            G2C_StartMatch_Ack response = new G2C_StartMatch_Ack();
            try {
                // 验证Session
                if (!GateHelper.SignSession(session)) {
                    response.Error = ErrorCode.ERR_SignError;
                    reply(response);
                    return;
                }
                User user = session.GetComponent<SessionUserComponent>().User;
                // 验证玩家是否符合进入房间要求,默认为100底分局
                RoomConfig roomConfig = RoomHelper.GetConfig(RoomLevel.Lv100);
                // 【数据库接入问题】：数据库还没有接好，暂时不验证钱的多少
                // UserInfo userInfo = await Game.Scene.GetComponent<DBProxyComponent>().Query<UserInfo>(user.UserID, false); // 跑数据库里去拿，这个玩家的现金验证是否合格
                // if (userInfo.Money < roomConfig.MinThreshold) {
                //     response.Error = ErrorCode.ERR_UserMoneyLessError; // 玩家钱不够，不能玩
                //     reply(response);
                //     return;
                // }
// 这里先发送响应，让客户端收到后切换房间界面，否则可能会出现重连消息在切换到房间界面之前发送导致重连异常【这个应该是，别人的源标注了】
// 这里的顺序就显得关键：因为只有网关服向客户端返回服务器的匹配响应【并不一定说已经匹配完成，但告诉客户端服务器在着手处理这个工作。。。】，客户端才能创建房间UI 控件
                reply(response); 
// 向匹配服务器发送匹配请求: 【路由器系统】ET7 重构后的路由器系统还没有弄懂。现在拿不到匹配服的地址
                StartConfigComponent config = Game.Scene.GetComponent<StartConfigComponent>();
                IPEndPoint matchIPEndPoint = config.MatchConfig.GetComponent<InnerConfig>().IPEndPoint; // 匹配服务器的远程IP 地址
                // Session matchSession = Game.Scene.GetComponent<NetInnerComponent>().Get(matchIPEndPoint); // 拿到与这个匹配服务器通信的会话框实例
                Session matchSession = NetInnerComponentSystem.Get(matchIPEndPoint);
                M2G_PlayerEnterMatch_Ack m2G_PlayerEnterMatch_Ack = await matchSession.Call(new G2M_PlayerEnterMatch_Req() { // 发消息代为客户端申请：申请匹配游戏
                        PlayerID = user.InstanceId,
                            UserID = user.UserID,
                            SessionID = session.InstanceId,
                            }) as M2G_PlayerEnterMatch_Ack;
                user.IsMatching = true;
            } 
            catch (Exception e) {
                ReplyError(response, e, reply);
            }
        }
    }
}