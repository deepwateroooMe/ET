using System;
using ET;
namespace ET.Server {

    [MessageHandler(SceneType.Gate)]
    public class C2G_PingHandler : AMRpcHandler<C2G_Ping, G2C_Ping> {

		// protected override async ETTask Run(Session session, C2G_Ping message, Action<G2C_Ping> reply) {
		protected override async ETTask Run(Session session, C2G_Ping request, G2C_Ping response) {
            G2C_Ping response = new G2C_Ping();
            response.Time = TimeHelper.ServerNow();
            // reply(response); // 嘲笑嘲笑自己：亲爱的表哥的活宝妹，一两个周之前不懂瞎改的弱弱。。。
            await ETTask.CompletedTask; // 这个是说，等上面的回复回调执行完毕？
        }
	}
}