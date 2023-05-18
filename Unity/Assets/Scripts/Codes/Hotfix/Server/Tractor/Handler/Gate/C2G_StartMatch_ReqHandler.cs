using System;
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
// // 向匹配服务器发送匹配请求: 【路由器系统】ET7 重构后的路由器系统还没有弄懂。现在拿不到匹配服的地址.
                Scene scene = session.DomainScene();
                RouterAddressComponent routerAddressComponent = scene.GetComponent<RouterAddressComponent>(); // 拿的是【网关服】的这个路由器组件
                IPEndPoint realmAddress = RouterAddressComponentSystem.GetMatchAddress(scene.GetComponent<PlayerComponent>().Get(user.UserID).Account); // 随机分配一个，取模分配
                Session matchSession = Root.Instance.Scene.GetComponent<NetInnerComponent>().Get(matchIPEndPoint); // 应该还是用这个组件去拿
                // Session matchSession = NetInnerComponentSystem.Get(matchIPEndPoint);

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
//             try { // 贴这里：供自己参考 LoginHelper.cs 【客户端逻辑】
//                 // 创建一个ETModel层的Session. 【没看懂】：如何区分不同层，为什么先移去，又添加？
// // 这个组件：热更新域与常规域有不同吗? 尽管可能会没有不同，但它是实时的，就是过程中可能会有服务器掉线？所以取最新的？
//                 clientScene.RemoveComponent<RouterAddressComponent>(); 
//                 // 获取路由跟realmDispatcher地址
//                 RouterAddressComponent routerAddressComponent = clientScene.GetComponent<RouterAddressComponent>();
//                 if (routerAddressComponent == null) {
//                     routerAddressComponent = clientScene.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
//                     await routerAddressComponent.Init();
//                     clientScene.AddComponent<NetClientComponent, AddressFamily>(routerAddressComponent.RouterManagerIPAddress.AddressFamily);
//                 }
//                 IPEndPoint realmAddress = routerAddressComponent.GetRealmAddress(account);
//                 R2C_Login r2CLogin;
//                 using (Session session = await RouterHelper.CreateRouterSession(clientScene, realmAddress)) {
//                     r2CLogin = (R2C_Login) await session.Call(new C2R_Login() { Account = account, Password = password });
//                 }
//                 // 创建一个gate Session,并且保存到SessionComponent中: 与网关服的会话框。主要负责用户下线后会话框的自动移除销毁
//                 Session gateSession = await RouterHelper.CreateRouterSession(clientScene, NetworkHelper.ToIPEndPoint(r2CLogin.Address));
//                 clientScene.AddComponent<SessionComponent>().Session = gateSession;
//                 G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(
//                     new C2G_LoginGate() { Key = r2CLogin.Key, GateId = r2CLogin.GateId});
//                 Log.Debug("登陆gate成功!");
//                 await EventSystem.Instance.PublishAsync(clientScene, new EventType.LoginFinish());
//             }
//             catch (Exception e) {
//                 Log.Error(e);
//             }
        }
    }
}