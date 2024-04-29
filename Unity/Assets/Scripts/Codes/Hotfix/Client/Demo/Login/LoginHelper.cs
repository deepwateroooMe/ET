using System;
using System.Net;
using System.Net.Sockets;
namespace ET.Client {
    public static class LoginHelper {
		// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】

		// 感觉，这些框架示范项目的登录逻辑等，其实都熟悉透彻了，一看就懂
        public static async ETTask Login(Scene clientScene, string account, string password) {
            try {
                // 创建一个ETModel层的Session
                clientScene.RemoveComponent<RouterAddressComponent>();
                // 获取路由跟realmDispatcher地址
                RouterAddressComponent routerAddressComponent = clientScene.GetComponent<RouterAddressComponent>();
                if (routerAddressComponent == null) {
					// 【客户端】：RouterHttpHost 等，都是作为常数值传入的
                    routerAddressComponent = clientScene.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
                    await routerAddressComponent.Init();
                    
                    clientScene.AddComponent<NetClientComponent, AddressFamily>(routerAddressComponent.RouterManagerIPAddress.AddressFamily);
                }
                IPEndPoint realmAddress = routerAddressComponent.GetRealmAddress(account);
                
                R2C_Login r2CLogin;
                using (Session session = await RouterHelper.CreateRouterSession(clientScene, realmAddress)) {
                    r2CLogin = (R2C_Login) await session.Call(new C2R_Login() { Account = account, Password = password });
                }
                // 创建一个gate Session,并且保存到SessionComponent中【源】：下面Address 应该是，为当前客户端，所随机分配的【网关服】的地址
                Session gateSession = await RouterHelper.CreateRouterSession(clientScene, NetworkHelper.ToIPEndPoint(r2CLogin.Address));
				// 【客户端】场景：基本只持一个，与【网关服】的【会话框】。网关服成客户端总代理，客户端一切经网关服转发。那更底层，仍走【动态软路由】安全防攻击
                clientScene.AddComponent<SessionComponent>().Session = gateSession;
                
                G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(
                    new C2G_LoginGate() { Key = r2CLogin.Key, GateId = r2CLogin.GateId});
                Log.Debug("登陆gate成功!");
				// 逻辑，从这里，继续往下一个UI 控件推进
                await EventSystem.Instance.PublishAsync(clientScene, new EventType.LoginFinish());
            }
            catch (Exception e) {
                Log.Error(e);
            }
        } 
    }
}