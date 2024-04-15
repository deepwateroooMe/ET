using System;
namespace ET {
    public interface IEvent {
        Type Type { get; }
    }
    public abstract class AEvent<S, A>: IEvent where S: class, IScene where A: struct {
        public Type Type {
            get {
                return typeof (A);
            }
        }
        protected abstract ETTask Run(S scene, A a); // 抽象方法：供实体类实现
        public async ETTask Handle(S scene, A a) {   // 一层包装，调用子类的实体实现方法
            try {
                await Run(scene, a);
            }
            catch (Exception e) {
                Log.Error(e);
            }
        }
    }
}