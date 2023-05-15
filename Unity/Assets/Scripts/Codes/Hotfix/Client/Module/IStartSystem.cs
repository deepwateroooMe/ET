using System;
namespace ET.Client {
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
        InstanceQueueIndex ISystemType.GetInstanceQueueIndex() { // 这里没看懂在干什么，大概还有个地方，我得去改
            return InstanceQueueIndex.Start; 
        }
        protected abstract void Start(T self);
    }
}