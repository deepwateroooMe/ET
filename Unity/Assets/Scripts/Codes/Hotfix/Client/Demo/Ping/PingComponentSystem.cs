using System;
namespace ET.Client {
	// 【路由器心跳包】：网络中的路由器，可能会被黑客攻击。。如亲爱的表哥的活宝妹，现在总被、一再被、亲爱的表哥的活宝妹住处的极端奸佞太监 gay 杀人猪贱鸡、贱畜牲、一再陷害、暗杀、杀人！！它真贱！！
	
    [ObjectSystem]
    public class PingComponentAwakeSystem: AwakeSystem<PingComponent> {
        protected override void Awake(PingComponent self) {
            PingAsync(self).Coroutine();
        }
        private static async ETTask PingAsync(PingComponent self) {
            Session session = self.GetParent<Session>();
            long instanceId = self.InstanceId;
            while (true) {
                if (self.InstanceId != instanceId) {
                    return;
                }
                long time1 = TimeHelper.ClientNow();
                try {
					// 通过路由器中转的会话框：每2 秒钟，向【网关服】发一个心跳消息，目的是？路由器可能被攻击；客户端可能掉线？
                    G2C_Ping response = await session.Call(new C2G_Ping()) as G2C_Ping;
                    if (self.InstanceId != instanceId) {
                        return;
                    }
                    long time2 = TimeHelper.ClientNow();
                    self.Ping = time2 - time1;
                    
                    TimeInfo.Instance.ServerMinusClientTime = response.Time + (time2 - time1) / 2 - time2;
                    await TimerComponent.Instance.WaitAsync(2000);
                }
                catch (RpcException e) {
                    // session断开导致ping rpc报错，记录一下即可，不需要打成error
                    Log.Info($"ping error: {self.Id} {e.Error}");
                    return;
                }
                catch (Exception e) {
                    Log.Error($"ping error: \n{e}");
                }
            }
        }
    }
    [ObjectSystem]
    public class PingComponentDestroySystem: DestroySystem<PingComponent> {
        protected override void Destroy(PingComponent self) {
            self.Ping = default;
        }
    }
}