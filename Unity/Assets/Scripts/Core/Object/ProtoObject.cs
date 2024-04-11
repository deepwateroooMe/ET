using System;
using System.ComponentModel;
namespace ET {
    public abstract class ProtoObject: Object, ISupportInitialize { // 抽象类 
        public object Clone() { // 【TODO】：方法没看懂
            byte[] bytes = SerializeHelper.Serialize(this);
			// 下面：即刻，反序列化，什么意思呢？
			// 下面，反序列化，找不到任何【跨进程消息传递】相关的逻辑；去找上面，序列化后干过什么？
			// 库里，2 个方法的底层细节是不懂；但是，【跨进程消息传递】相关的逻辑，不该在这里找；去帮助项目ExcelExporter 里IMerge 合并跨进程消息里去找。【TODO】：
            return SerializeHelper.Deserialize(this.GetType(), bytes, 0, bytes.Length);
        }
        public virtual void BeginInit() {
        }
        
        public virtual void EndInit() {
        }
        
        public virtual void AfterEndInit() { // ET 框架，从封装了一个便利接口
        }
    }
}