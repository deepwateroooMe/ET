using System;
using System.ComponentModel;
using System.IO;
using ProtoBuf.Meta;
using Unity.Mathematics;
namespace ET {

    public static class ProtobufHelper { // 双端共享
        public static void Init() { } // 占位符

        static ProtobufHelper() { // 把客户端Unity 里可能用到的数据类型，跨进程可认识公认
            RuntimeTypeModel.Default.Add(typeof(float2), false).Add("x", "y");
            RuntimeTypeModel.Default.Add(typeof(float3), false).Add("x", "y", "z");
            RuntimeTypeModel.Default.Add(typeof(float4), false).Add("x", "y", "z", "w");
            RuntimeTypeModel.Default.Add(typeof(quaternion), false).Add("value");
        }

        public static object Deserialize(Type type, byte[] bytes, int index, int count) {
            using MemoryStream stream = new MemoryStream(bytes, index, count); // 内存流出来了：【内存流上、发消息】＝＝》消息会自动、出去跨进程吗？
            object o = ProtoBuf.Serializer.Deserialize(type, stream);
            if (o is ISupportInitialize supportInitialize) {
                supportInitialize.EndInit();
            }
            return o;
        }
        public static byte[] Serialize(object message) {
            using MemoryStream stream = new MemoryStream();
			// 感觉，下面，序列化的过程：底层，埋着，网络链接、合并IMerge 多个树节点般、维护着继承关系的、跨进程消息、自动合并的过程【TODO】：去确认这点儿！
            ProtoBuf.Serializer.Serialize(stream, message); // 这里：感觉，没能懂【ProtoBuf 跨进程消息传递、序列化与反序列化库】的底层原理
			// 看着，像是极简单，序列化一下，像没做什么。。。可是，水深深深。。。
			// 这里的问题，就变成是：消息，序列化到了，内存流上
			// 内存流：是什么？序列化到这个流上的消息，是操作系统的底层，一定会、发出去的吗？。。。
			// 序列化：对象序列化的最主要的用处就是在传递和保存对象的时候，保证对象的完整性和可传递性。序列化是把对象转换成有序字节流，
			// 【以便在网络上传输或者保存在本地文件中。核心作用是对象状态的保存与重建。】按这么看，也不一定、没保证，是一定会发出去的，也可能写到什么文件中。。。。
			// 那么，亲爱的表哥的活宝妹，可以去找：框架里，ProtoBuf 的初始化中，哪里是否配置过，把序列化后的【配置相关消息】写入到什么文件中
            return stream.ToArray();
        }
        public static void Serialize(object message, Stream stream) {
            ProtoBuf.Serializer.Serialize(stream, message);
        }
        public static object Deserialize(Type type, Stream stream) {
            object o = ProtoBuf.Serializer.Deserialize(type, stream);
            if (o is ISupportInitialize supportInitialize) {
                supportInitialize.EndInit();
            }
            return o;
        }
    }
}