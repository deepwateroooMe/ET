using System;
namespace ET {
    public interface IInvoke { // 接口申明  
        Type Type { get; }
    }
    public abstract class AInvokeHandler<A>: IInvoke where A: struct { // 抽象类：双端【服务端、客户端】会有各自的、实体实现类
        public Type Type {
            get {
                return typeof (A);
            }
        }
        public abstract void Handle(A a); // 无返回、Invoke 调用 
    }
    public abstract class AInvokeHandler<A, T>: IInvoke where A: struct {
        public Type Type {
            get {
                return typeof (A);
            }
        }
        public abstract T Handle(A a); // 有返回泛型 T 的 Invoke 调用
    }
}