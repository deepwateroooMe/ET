using System;
namespace ET {
	// 【普通接口】与【泛型接口】：反序列化接口，一定程序上实现了自动化。一点儿底层的便利封装。具备扩展性
	// 【TODO】：找不到框架里，真正实现这些接口的地方
    public interface IDeserialize { // 框架里，各接口之一 
    }
    public interface IDeserializeSystem: ISystemType {
        void Run(Entity o); // 自动【反序列化】函数：搭桥穿线自动化封装
    }

    // 反序列化后执行的System
    [ObjectSystem]
    public abstract class DeserializeSystem<T> : IDeserializeSystem where T: Entity, IDeserialize {
        void IDeserializeSystem.Run(Entity o) {
            this.Deserialize((T)o); // 就是调用执行【反序列化】成指定的【泛型类型】对象
        }
        Type ISystemType.SystemType() {
            return typeof(IDeserializeSystem);
        }
        int ISystemType.GetInstanceQueueIndex() { // 缺省吗？可重赋值与修改的吧
            return InstanceQueueIndex.None;
        }
        Type ISystemType.Type() {
            return typeof(T);
        }
        protected abstract void Deserialize(T self); // 【反序列化】：具体逻辑，可扩展、多样化实现
    }
}
