using System;
namespace ET.Server {
    [ObjectSystem]
    public class LocationProxyComponentAwakeSystem: AwakeSystem<LocationProxyComponent> {
        protected override void Awake(LocationProxyComponent self) {
            LocationProxyComponent.Instance = self;
        }
    }
    [ObjectSystem]
    public class LocationProxyComponentDestroySystem: DestroySystem<LocationProxyComponent> {
        protected override void Destroy(LocationProxyComponent self) {
            LocationProxyComponent.Instance = null;
        }
    }
    // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
    public static class LocationProxyComponentSystem { // 【位置代理组件】：以前都没仔细看过，看来今天不得不都多看看了。
        private static long GetLocationSceneId(long key) {
            return StartSceneConfigCategory.Instance.LocationConfig.InstanceId;
        }
        // 就从这个方法看：从场景中，发了一条 ObjectAddRequest() 消息出去。去找，哪个服【位置服】会处理这个请求消息，如何处理的？
        public static async ETTask Add(this LocationProxyComponent self, long key, long instanceId) {
            Log.Info($"location proxy add {key}, {instanceId} {TimeHelper.ServerNow()}");
            await ActorMessageSenderComponent.Instance.Call(GetLocationSceneId(key),// 【位置服】有个专门的类，来处理 
                      new ObjectAddRequest() { Key = key, InstanceId = instanceId });
        }
        public static async ETTask Lock(this LocationProxyComponent self, long key, long instanceId, int time = 60000) {
            Log.Info($"location proxy lock {key}, {instanceId} {TimeHelper.ServerNow()}");
            await ActorMessageSenderComponent.Instance
                .Call(GetLocationSceneId(key),
                      new ObjectLockRequest() { Key = key, InstanceId = instanceId, Time = time });
        }
        public static async ETTask UnLock(this LocationProxyComponent self, long key, long oldInstanceId, long instanceId) {
            Log.Info($"location proxy unlock {key}, {instanceId} {TimeHelper.ServerNow()}");
            await ActorMessageSenderComponent.Instance
                .Call(GetLocationSceneId(key),
                      new ObjectUnLockRequest() { Key = key, OldInstanceId = oldInstanceId, InstanceId = instanceId });
        }
        public static async ETTask Remove(this LocationProxyComponent self, long key) {
            Log.Info($"location proxy add {key}, {TimeHelper.ServerNow()}");
            await ActorMessageSenderComponent.Instance
                .Call(GetLocationSceneId(key),
                      new ObjectRemoveRequest() { Key = key });
        }
        public static async ETTask<long> Get(this LocationProxyComponent self, long key) {
            if (key == 0) {
                throw new Exception($"get location key 0");
            }
            // location server配置到共享区，一个大战区可以配置N多个location server,这里暂时为1
            // 去找：【小服】的处理逻辑
            ObjectGetResponse response =
                (ObjectGetResponse) await ActorMessageSenderComponent.Instance.Call(GetLocationSceneId(key),
                                                                                    new ObjectGetRequest() { Key = key });
            return response.InstanceId;
        }
        public static async ETTask AddLocation(this Entity self) {
            await LocationProxyComponent.Instance.Add(self.Id, self.InstanceId);
        }
        public static async ETTask RemoveLocation(this Entity self) {
            await LocationProxyComponent.Instance.Remove(self.Id);
        }
    }
}
