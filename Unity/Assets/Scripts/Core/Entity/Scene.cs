using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;
namespace ET {
    [EnableMethod]
    [ChildOf]
    public class Scene: Entity, IScene { // 像是对，场景与纤程的组装
		// 自ET7 后，每核进程可以有多个场景多个线程。新版本里拿纤程当线程用，也就每个场景有个线程纤程
        [BsonIgnore]
        public Fiber Fiber { get; set; } // 纤程线程
        public string Name { get; }
        public SceneType SceneType {
            get;
            set;
        }
        public Scene() {
        }
        public Scene(Fiber fiber, long id, long instanceId, SceneType sceneType, string name) {
            this.Id = id;
            this.Name = name;
            this.InstanceId = instanceId;
            this.SceneType = sceneType;
            this.IsCreated = true;
            this.IsNew = true;
            this.Fiber = fiber;
            this.IScene = this;
            this.IsRegister = true;
            Log.Info($"scene create: {this.SceneType} {this.Id} {this.InstanceId}");
        }
        public override void Dispose() {
            base.Dispose();
            Log.Info($"scene dispose: {this.SceneType} {this.Id} {this.InstanceId}");
        }
        protected override string ViewName {
            get {
                return $"{this.GetType().Name} ({this.SceneType})";
            }
        }
    }
}