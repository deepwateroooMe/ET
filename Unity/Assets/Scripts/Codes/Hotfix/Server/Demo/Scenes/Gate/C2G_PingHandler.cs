using System;
using ET;
namespace ET.Server {

    [MessageHandler(SceneType.Gate)]
    public class C2G_PingHandler : AMRpcHandler<C2G_Ping, G2C_Ping> {

		protected override async void Run(Session session, C2G_Ping message, Action<G2C_Ping> reply) {
            G2C_Ping response = new G2C_Ping();
            response.Time = TimeHelper.ServerNow();
            reply(response);
            await ETTask.CompletedTask; // 这个是说，等上面的回复回调执行完毕？
        }
	}
}