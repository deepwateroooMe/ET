using System;
namespace ET {
	// 界面 Interface：这是对几大回调的底层抽象
    public interface ILoad {
    }
    public interface ILoadSystem: ISystemType {
        void Run(Entity o);
    }
    [ObjectSystem]
    public abstract class LoadSystem<T> : ILoadSystem where T: Entity, ILoad {
        void ILoadSystem.Run(Entity o) {
            this.Load((T)o);
        }
        Type ISystemType.Type() {
            return typeof(T);
        }
        Type ISystemType.SystemType() {
            return typeof(ILoadSystem);
        }
        int ISystemType.GetInstanceQueueIndex() {
            return InstanceQueueIndex.Load;
        }
        protected abstract void Load(T self);
    }
}
