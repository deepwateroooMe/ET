using System;
namespace ET {
    public interface IStart { }

    public interface IStartSystem : ISystemType {
        void Run(Entity o);
    }

    [ObjectSystem]
    public abstract class StartSystem<T> : IStartSystem where T: Entity, IStart {
        void IStartSystem.Run(Entity o) {
            this.Start((T)o);
        }
        Type ISystemType.Type() {
            return typeof(T);
        }
        Type ISystemType.SystemType() {
            return typeof(IStartSystem);
        }
        InstanceQueueIndex ISystemType.GetInstanceQueueIndex() { // 要添加进某个系统，这里早改完了
            return InstanceQueueIndex.Start; 
        }
        protected abstract void Start(T self);
    }
}