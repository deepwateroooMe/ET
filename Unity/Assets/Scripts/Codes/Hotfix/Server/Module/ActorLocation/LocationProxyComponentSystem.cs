using System;
namespace ET.Server { // 说是【位置服代理】，实际是个帮助类Helper，帮助链接，包装异步任务等，将网络异步操作发向【位置服】
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
    public static class LocationProxyComponentSystem { // 【位置代理组件】：先去确认，框架里使用了这个代理组件？
        private static long GetLocationSceneId(long key) { // 拿【位置服】：位置场景，实例标记号 
// StartSceneConfigCategory: 全局单例，就是亲爱的表哥，就是活宝妹的领海神灯：是中央邮政，是一定范围相对大范围（同进程上？）可见，方便索引（注册更新查询）
            return StartSceneConfigCategory.Instance.LocationConfig.InstanceId;
        }
        // 就从这个方法看：从场景中，发了一条 ObjectAddRequest() 消息出去。去找，哪个服【位置服！】会处理这个请求消息，如何处理的？位置服生成系文件补充完整
        public static async ETTask Add(this LocationProxyComponent self, long key, long instanceId) { // 向中央邮政【注册】：一个实例号， key 是？
            Log.Info($"location proxy add {key}, {instanceId} {TimeHelper.ServerNow()}");
            // 发条跨进程消息：发向【位置服】实例标记号，使用跨进程发送消息组件，将位置（注册更新查询）消息，发向【位置服】
            await ActorMessageSenderComponent.Instance.Call(GetLocationSceneId(key),// 【位置服】有个专门的类，来处理一个类型的消息。上次看到位置服不完整没再看 
                                                            new ObjectAddRequest() { Key = key, InstanceId = instanceId }); // 以前没细看，消息 actorId 都可以自动去掉，封装在框架哪里？消息明明是定义为三个参数的
        }
        // 【位置服】生成系：文件待补。小伙伴搬家云游时，位置不定，是需要上报中央给上锁通告不便查询的；结束了仍旧上报中央通告，确定的位置信息
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
        // 下面两个方法：添加和移除的是，代理的位置信息？没明白这里的管理层级结构
        public static async ETTask AddLocation(this Entity self) {
            await LocationProxyComponent.Instance.Add(self.Id, self.InstanceId);
        }
        public static async ETTask RemoveLocation(this Entity self) {
            await LocationProxyComponent.Instance.Remove(self.Id);
        }
    }
} // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
