using System;
namespace ET {
	// EntityRef: 大概涉及一个：大型游戏服、分服分线等的概念。就是玩家，可以从6 区登出，从7 区登入，实体 entity 可能不变，但 instanceId 实例号是变化的
    public readonly struct EntityRef<T> where T: Entity {
        private readonly long instanceId;
        private readonly T entity;
        private EntityRef(T t) {
            this.instanceId = t.InstanceId;
            this.entity = t;
        }
        private T UnWrap {
            get {
                if (this.entity == null) {
                    return null;
                }
                if (this.entity.InstanceId != this.instanceId) {
                    return null;
                }
                return this.entity;
            }
        }
        public static implicit operator EntityRef<T>(T t) {
            return new EntityRef<T>(t);
        }
        public static implicit operator T(EntityRef<T> v) {
            return v.UnWrap;
        }
    }
}