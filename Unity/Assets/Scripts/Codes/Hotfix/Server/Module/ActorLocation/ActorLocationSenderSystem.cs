using System;
namespace ET.Server {
    //【重复】：感觉这个类，与其它 ActorLocationSenderComponentSystem 有重复，应该是可以删除的
    [ObjectSystem]
    public class ActorLocationSenderAwakeSystem: AwakeSystem<ActorLocationSender> {
        protected override void Awake(ActorLocationSender self) {
            self.LastSendOrRecvTime = TimeHelper.ServerNow();
            self.ActorId = 0;
            self.Error = 0;
        }
    }
    [ObjectSystem]
    public class ActorLocationSenderDestroySystem: DestroySystem<ActorLocationSender> {
        protected override void Destroy(ActorLocationSender self) {
            Log.Debug($"actor location remove: {self.Id}");
            self.LastSendOrRecvTime = 0;
            self.ActorId = 0;
            self.Error = 0;
        }
    }
}