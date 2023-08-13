using System;
using System.ComponentModel;
namespace ET {
    public abstract class ProtoObject: Object, ISupportInitialize {

        public object Clone() { // 【进程间可传递的消息】：为什么这里的复制过程，是先序列化，再反序列化？框架里，也找不到真正调用它的地方
            // 【复制：序列化与反序列化】复制跨进程的消息，【复制】，实际是要传一个版本跨进程到其它进程，那么就需要【序列化】到内存流、内存流上发消息、【反序列化】读取（这里想得未必对）
            // 消息明明就是反序列化好的，为什么再来一遍？得【序列化、反序列化】过程到其它进程。【序列化】到内存流的过程，若是内存流缓过过的最后一条消息，序列化步骤可短路跳过
                // 在底层内存流上的反序列化方法时（ProtobufHelper.Deserialize()），会调用 ISupportInitialize 的EndInit()回调，反序列化后可做的事的回调
                // 序列化前的回调，是哪里调用的？BeginInit() 回调在框架里，只有在MongoHelper.cs 的Json 序列化前，会调用；ProtoBuf 序列化前，不曾注册过这个回调
            // 上下两句：紧接着。。。并没有跨进程什么？？？【这个方法，没能看懂】 
            byte[] bytes = SerializeHelper.Serialize(this);
            return SerializeHelper.Deserialize(this.GetType(), bytes, 0, bytes.Length);
        }
        public virtual void BeginInit() {
        }
        public virtual void EndInit() {
        }
        public virtual void AfterEndInit() { // 这个回调，与上一个 EndInit() 区别是？试着去找一个例子出来看看
        }
    }
}