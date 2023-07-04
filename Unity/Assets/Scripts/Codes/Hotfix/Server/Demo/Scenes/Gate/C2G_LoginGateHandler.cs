using System;
using ET;
namespace ET.Server {
    [MessageHandler(SceneType.Gate)]
    public class C2G_LoginGateHandler : AMRpcHandler<C2G_LoginGate, G2C_LoginGate> {
		protected override async ETTask Run(Session session, C2G_LoginGate request, G2C_LoginGate response) {
            string account = Root.Instance.Scene.GetComponent<GateSessionKeyComponent>().Get(request.Key);
            if (account == null) {
                response.Error = ErrorCore.ERR_ConnectGateKeyError;
                response.Message = "Gate key验证失败!";
                return;
            }
            // 【客户端逻辑】：客户端验证结束后，就可以删除这个组件了，服务端防挂防盗窃的
            session.RemoveComponent<SessionAcceptTimeoutComponent>();
            
// 这个场景的获取：Root.Instance.Scene. 是后来我自己改的，不一定对
            PlayerComponent playerComponent = Root.Instance.Scene.GetComponent<PlayerComponent>(); 
// <<<<<<<<<<<<<<<<<<<< 主要是这里添加新玩家的方法： 创建了一个新玩家：调用基类Entity 里 AddChild 方法
            // Entity.cs 里：创建新实例，或对象池里去取一个，会Id生成器会生成特异性身份号等。应该算是最底层的原理了
            Player player = playerComponent.AddChild<Player, string>(account); 
            // 将生成的新玩家添加到，需要对其进行管理的各管理类组件 PlayerComponent|SessionPlayerComponent
            playerComponent.Add(player);
            
            // 对新生成玩家的【通信会话框】进行管理：使其具备收发消息功能。会话框与玩家一一对应
            session.AddComponent<SessionPlayerComponent>().PlayerId = player.Id;
            session.AddComponent<MailBoxComponent, MailboxType>(MailboxType.GateSession);
            
            response.PlayerId = player.Id; // 写回复消息，玩家特异身份证传客户端
            await ETTask.CompletedTask;
        }
	}
}
