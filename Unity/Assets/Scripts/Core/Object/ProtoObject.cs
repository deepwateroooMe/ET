using System;
using System.ComponentModel;
namespace ET {
    public abstract class ProtoObject: Object, ISupportInitialize {

        public object Clone() { // 【进程间可传递的消息】：为什么这里的复制过程，是先序列化，再反序列化？
            // 不明白：消息明明就是反序列化好的，为什么再来一遍：序列化、反序列化（虽然这个再一遍的过程是 ProtoBuf 里的序列化与反序列化方法）？
            // 翻到Protobuf 里的反序列化方法，去查看：ET 框架的封装里，
                // 在底层内存流上的反序列化方法时（ProtobufHelper.Deserialize()），会调用 ISupportInitialize 的EndInit()回调，序列化后可做的事的回调
                // 序列化前的回调，是哪里调用的？BeginInit() 回调在框架里，只有在MongoHelper.cs 的Json 序列化前，会调用；ProtoBuf 序列化前好像跳过了这个回调
                // 就是提供了两个接口：调用与不调用，还是分不同的序列化工具
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