using System;
namespace ET {
    public interface IEvent {
        Type Type { get; } // 所有的回调事件，以使用上下文的场景，以及这个类型相区分。这个类型是必须的，所以封装在最底层
    }
    public abstract class AEvent<A>: IEvent where A: struct { // 抽象基类，供继承
        public Type Type {
            get {
                return typeof (A);
            }
        }
        protected abstract ETTask Run(Scene scene, A a); // 提供抽象接口，供实现
        
        public async ETTask Handle(Scene scene, A a) {
            try {
                await Run(scene, a);
            }
            catch (Exception e) { // 抽象蕨类包装了抛异步，各个子类就不必再无数次地重复同样的逻辑了呀
                Log.Error(e);
            }
        }
    }
}