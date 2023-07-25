using System;
using System.Net;
using System.Net.Sockets;
namespace ET.Client {
    public static class LoginHelper { // 程序域：热更新域在。调用自热更新视图层
        public static async ETTask Login(Scene clientScene, string account, string password) {
            try {
                // 创建一个ETModel层的Session.
// 这个组件：它的热更域里，好像有每 10 分钟再扫刷新一遍服务端系统；这里为什么必须先移除一遍，再添加一遍？
                // 是因为现客户端正在试图重新登录，说明先前登出了、掉线了、或是用户自己其它客户端顶号了，先前的这个组件，过期了，该回收
                clientScene.RemoveComponent<RouterAddressComponent>(); // 这里先删除，再去读：删除的过程是同步方法，不需要异步等待
                // 获取路由跟realmDispatcher地址
                RouterAddressComponent routerAddressComponent = clientScene.GetComponent<RouterAddressComponent>(); // 它可以神奇地自己添加。。它有个无限循环？忘记了，再去看一遍
                if (routerAddressComponent == null) {
                    routerAddressComponent = clientScene.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
                    await routerAddressComponent.Init();
                    // 为【客户端场景】：添加【网络客户端】组件。添加了这个组件，客户端场景才可以与各服务端交通（注册必要的事件订阅与监听），收发消息等
                    clientScene.AddComponent<NetClientComponent, AddressFamily>(routerAddressComponent.RouterManagerIPAddress.AddressFamily);
                }
                IPEndPoint realmAddress = routerAddressComponent.GetRealmAddress(account);
                R2C_Login r2CLogin;
                using (Session session = await RouterHelper.CreateRouterSession(clientScene, realmAddress)) {
                    // 这里，先去细看一下，写在 R2C_Login 里的返回消息内容是些什么？去找请求消息的服务端处理器
                    r2CLogin = (R2C_Login) await session.Call(new C2R_Login() { Account = account, Password = password });
                }
                // 创建一个gate Session,并且保存到SessionComponent中: 与网关服的会话框。主要负责用户下线后会话框的自动移除销毁
                Session gateSession = await RouterHelper.CreateRouterSession(clientScene, NetworkHelper.ToIPEndPoint(r2CLogin.Address));
                clientScene.AddComponent<SessionComponent>().Session = gateSession;
                
                G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(
                    new C2G_LoginGate() { Key = r2CLogin.Key, GateId = r2CLogin.GateId});
                Log.Debug("登陆gate成功!");
                await EventSystem.Instance.PublishAsync(clientScene, new EventType.LoginFinish());
            }
            catch (Exception e) {
                Log.Error(e);
            }
        } 
    }
}