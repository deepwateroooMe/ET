using System;
namespace ET {
    public interface ISystemType {
        Type Type();
        Type SystemType();
        // int GetInstanceQueueIndex();
        InstanceQueueIndex GetInstanceQueueIndex();
    }
}