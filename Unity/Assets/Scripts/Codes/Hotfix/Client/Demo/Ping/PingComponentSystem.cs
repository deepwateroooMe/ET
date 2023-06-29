using System;
namespace ET.Client {
    [ObjectSystem]
    public class PingComponentAwakeSystem: AwakeSystem<PingComponent> {
        protected override void Awake(PingComponent self) {
            PingAsync(self).Coroutine(); // 只要一个会话框添加了这个组件：就始终轮循。。。
        }
        private static async ETTask PingAsync(PingComponent self) {
            Session session = self.GetParent<Session>();
            long instanceId = self.InstanceId; // 【初始值：】 instanceId, 唯一正确的。如同，亲爱的表哥在活宝妹这里，是永远的单例存在，呵呵呵！！！
            while (true) { // 一个 true 可以是转了千年之后。。。
                if (self.InstanceId != instanceId)  // 所以要检查一遍：不一样就不对
                    return;
                long time1 = TimeHelper.ClientNow();
                try {
                    G2C_Ping response = await session.Call(new C2G_Ping()) as G2C_Ping; // 这里还是发给【网关服】的
                    if (self.InstanceId != instanceId) // 这里，又检查了一遍。。。 
                        return;
                    long time2 = TimeHelper.ClientNow();
                    self.Ping = time2 - time1;
                    TimeInfo.Instance.ServerMinusClientTime = response.Time + (time2 - time1) / 2 - time2;
                    await TimerComponent.Instance.WaitAsync(2000); // 每 2 秒，发一个最简心跳消息
                }
                catch (RpcException e) {
                    // session断开导致ping rpc报错，记录一下即可，不需要打成error
                    Log.Info($"ping error: {self.Id} {e.Error}");
                    return;
                } catch (Exception e) {
                    Log.Error($"ping error: \n{e}");
                }
            }
        }
    }
    [ObjectSystem]
    public class PingComponentDestroySystem: DestroySystem<PingComponent> {
        protected override void Destroy(PingComponent self) {
            self.Ping = default; // 不知道这个 default 是什么意思，先放一下
        }
    }
}
