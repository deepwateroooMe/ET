using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;
using Unity.Mathematics;
namespace ET {

    [ChildOf(typeof(UnitComponent))]
    [DebuggerDisplay("ViewName,nq")]
    public class Unit: Entity, IAwake<int> {
        public int ConfigId { get; set; } // 配置表id
        [BsonIgnore]
        public UnitConfig Config => UnitConfigCategory.Instance.Get(this.ConfigId);
        public UnitType Type => (UnitType)UnitConfigCategory.Instance.Get(this.ConfigId).Type;
        [BsonElement]
        private float3 position; // 坐标
        [BsonIgnore]
        public float3 Position {
            get => this.position;
            set {
                float3 oldPos = this.position;
                this.position = value;
                EventSystem.Instance.Publish(this.DomainScene(), new EventType.ChangePosition() { Unit = this, OldPos = oldPos });
            }
        }
        [BsonIgnore]
        public float3 Forward {
            get => math.mul(this.Rotation, math.forward());
            set => this.Rotation = quaternion.LookRotation(value, math.up());
        }
        [BsonElement]
        private quaternion rotation;
        [BsonIgnore]
        public quaternion Rotation {
            get => this.rotation;
            set {
                this.rotation = value;
                EventSystem.Instance.Publish(this.DomainScene(), new EventType.ChangeRotation() { Unit = this });
            }
        }
<<<<<<< HEAD
        protected override string ViewName {
            get {
                return $"{this.GetType().Name} ({this.Id})";
=======

        protected override string ViewName
        {
            get
            {
                return $"{this.GetType().FullName} ({this.Id})";
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
            }
        }
    }
}