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
    public static class LocationProxyComponentSystem {
        private static long GetLocationSceneId(long key) { // 这里是去拿：每个进程上、这个进程上的【位置服】场景的实例号
            return StartSceneConfigCategory.Instance.LocationConfig.InstanceId;
        }
		// 下面几个：向中央位置服数据库？CRUD 用户位置的【跨进程消息】封装，感觉还没看懂
        public static async ETTask Add(this LocationProxyComponent self, int type, long key, long instanceId) {
            Log.Info($"location proxy add {key}, {instanceId} {TimeHelper.ServerNow()}");
            await ActorMessageSenderComponent.Instance.Call(GetLocationSceneId(key),
                new ObjectAddRequest() { Type = type, Key = key, InstanceId = instanceId });
        }
		// 用这个方法作例子：把这几个类似方法的细节看懂
        public static async ETTask Lock(this LocationProxyComponent self, int type, long key, long instanceId, int time = 60000) {
            Log.Info($"location proxy lock {key}, {instanceId} {TimeHelper.ServerNow()}");
            await ActorMessageSenderComponent.Instance.Call(GetLocationSceneId(key),
                new ObjectLockRequest() { Type = type, Key = key, InstanceId = instanceId, Time = time });
			// 上面：去找ObjectLockRequest 这类跨进程消息的处理器，应该是中央位置服数据库什么之类的？
        }
        public static async ETTask UnLock(this LocationProxyComponent self, int type, long key, long oldInstanceId, long instanceId) {
            Log.Info($"location proxy unlock {key}, {instanceId} {TimeHelper.ServerNow()}");
            await ActorMessageSenderComponent.Instance.Call(GetLocationSceneId(key),
                new ObjectUnLockRequest() { Type = type, Key = key, OldInstanceId = oldInstanceId, InstanceId = instanceId });
        }
        public static async ETTask Remove(this LocationProxyComponent self, int type, long key) {
            Log.Info($"location proxy add {key}, {TimeHelper.ServerNow()}");
            await ActorMessageSenderComponent.Instance.Call(GetLocationSceneId(key),
                new ObjectRemoveRequest() { Type = type, Key = key });
        }
        public static async ETTask<long> Get(this LocationProxyComponent self, int type, long key) {
            if (key == 0) {
                throw new Exception($"get location key 0");
            }
            // location server配置到共享区，一个大战区可以配置N多个location server,这里暂时为1
			// GetLocationSceneId(key): 是去拿【同一进程上】的、LocationType 场景、位置小分服的实例号。就是、就近【查找最近的、位置服、分服、服务器分身】实例号
			// 【跨进程、向位置服的、位置查询】跨进程调用。位置服的处理逻辑，改天再看
            ObjectGetResponse response =
                    (ObjectGetResponse) await ActorMessageSenderComponent.Instance.Call(GetLocationSceneId(key),
                        new ObjectGetRequest() { Type = type, Key = key });
            return response.InstanceId;
        }
        public static async ETTask AddLocation(this Entity self, int type) {
            await LocationProxyComponent.Instance.Add(type, self.Id, self.InstanceId);
        }
        public static async ETTask RemoveLocation(this Entity self, int type) {
            await LocationProxyComponent.Instance.Remove(type, self.Id);
        }
    }
}