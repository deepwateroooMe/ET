using System;
using System.ComponentModel;
namespace ET {
    public abstract class ProtoObject: Object, ISupportInitialize {
        public object Clone() { // 【进程间可传递的消息】：为什么这里的复制过程，是先序列化，再反序列化？
            byte[] bytes = SerializeHelper.Serialize(this);
            return SerializeHelper.Deserialize(this.GetType(), bytes, 0, bytes.Length);
        }
        public virtual void BeginInit() {
        }
        public virtual void EndInit() {
        }
        public virtual void AfterEndInit() { // 这个回调，与上一个 EndInit() 区别是？
        }
    }
}