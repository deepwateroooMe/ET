using System;
using ET;
using System.Net;
namespace ET.Server {
// 网关服：处理客户端 StartMatch 请求消息. 消除了编译错误，不知道会不会有运行时错误！【爱表哥，爱生活！！！活宝妹就是一定要嫁给亲爱的表哥！！！】
    [MessageHandler(SceneType.Gate)] 
    public class C2G_StartMatch_ReqHandler : AMRpcHandler<C2G_StartMatch_Req, G2C_StartMatch_Ack> {
        protected override async ETTask Run(Session session, C2G_StartMatch_Req message, G2C_StartMatch_Ack response) {
            if (!GateHelper.SignSession(session)) { // 验证Session
                response.Error = ErrorCode.ERR_SignError;
                // reply(response);
                return;
            }
            User user = session.GetComponent<SessionUserComponent>().User;
            RoomConfig roomConfig = RoomHelper.GetConfig(RoomLevel.Lv100); // 验证玩家是否符合进入房间要求,默认为100底分局
            // 【数据库接入】：注意热更域里静态方法的调用，是需要传入实例的组件索引的，就是根场景下组件的索引
            UserInfo userInfo = await DBManagerComponentSystem.GetZoneDB(Root.Instance.Scene.GetComponent<DBManagerComponent>(), session.DomainZone()).Query<UserInfo>(user.UserID); // 跑数据库里去拿，这个玩家的现金验证是否合格
            // UserInfo userInfo = await Game.Scene.GetComponent<DBProxyComponent>().Query<UserInfo>(user.UserID, false); // 跑数据库里去拿，这个玩家的现金验证是否合格
            if (userInfo.Money < roomConfig.MinThreshold) {
                response.Error = ErrorCode.ERR_UserMoneyLessError; // 玩家钱不够，不能玩
                return;
            }
// 这里先发送响应，让客户端收到后切换房间界面，否则可能会出现重连消息在切换到房间界面之前发送导致重连异常【这个应该是，别人的源标注了】
// 这里的顺序就显得关键：因为只有网关服向客户端返回服务器的匹配响应【并不一定说已经匹配完成，但告诉客户端服务器在着手处理这个工作。。。】，客户端才能创建房间UI 控件
            // 【涉及到一定顺序的时候】：过里可能会存在一定的问题，因为ET7 的重新对回复消息的返回发送封装。就是这里是希望先把消息发回去，再处理下面的逻辑；但ET7 封装会先处理如下逻辑，再最后发返回消息。。。
            Scene scene = session.DomainScene(); // 这个例子，似乎看起来，是被自己改通了，可以用作参考
            StartSceneConfig config = RealmGateAddressHelper.GetMatch(session.DomainZone()); // 随机分配一个Match 匹配服。【这里没记的话，就任何时候，随机分配一个Match ？】
            Log.Debug($"match address: {MongoHelper.ToJson(config)}");
            M2G_PlayerEnterMatch_Ack m2G_PlayerEnterMatch_Ack = await scene.GetComponent<NetInnerComponent>().Get(config.InstanceId).Call(new G2M_PlayerEnterMatch_Req() { // 发消息代为客户端申请：申请匹配游戏
                    PlayerID = user.InstanceId,
                        UserID = user.UserID,
                        SessionID = session.InstanceId,
                        }) as M2G_PlayerEnterMatch_Ack;
            user.IsMatching = true;
        }
    }
}