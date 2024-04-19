using System;
namespace ET.Server {
	// 【ActorLocationSender】：它的，亲爱的表哥的活宝妹，呢称它：【小生成系】！它们，消息发送者的，各自的人生、生命周期。。。

	[ObjectSystem]
     public class ActorLocationSenderAwakeSystem: AwakeSystem<ActorLocationSender> {
        protected override void Awake(ActorLocationSender self) {
            self.LastSendOrRecvTime = TimeHelper.ServerNow();
            self.ActorId = 0; // 新创建时，是 0
            self.Error = 0;
        }
    }
    [ObjectSystem]
    public class ActorLocationSenderDestroySystem: DestroySystem<ActorLocationSender> {
        protected override void Destroy(ActorLocationSender self) {
            Log.Debug($"actor location remove: {self.Id}");
            self.LastSendOrRecvTime = 0;
            self.ActorId = 0; // 终老病死、尘归尘土归土时，也是 0
            self.Error = 0;
        }
    }
}