using System;
using System.ComponentModel;
using System.IO;
using ProtoBuf.Meta;
using Unity.Mathematics;

namespace ET {

    public static class ProtobufHelper {
        
        public static void Init() {
        }
        static ProtobufHelper() {
            // 序列化  反序列化  过程中,可能涉及的数据类型转换,初始化设置 
            RuntimeTypeModel.Default.Add(typeof(float2), false).Add("x", "y");
            RuntimeTypeModel.Default.Add(typeof(float3), false).Add("x", "y", "z");
            RuntimeTypeModel.Default.Add(typeof(float4), false).Add("x", "y", "z", "w");
            RuntimeTypeModel.Default.Add(typeof(quaternion), false).Add("value");
        }

        public static object Deserialize(Type type, byte[] bytes, int index, int count) {
            using MemoryStream stream = new MemoryStream(bytes, index, count);
            object o = ProtoBuf.Serializer.Deserialize(type, stream);
            if (o is ISupportInitialize supportInitialize) {
                supportInitialize.EndInit();
            }
            return o;
        }

        public static byte[] Serialize(object message) {
            using MemoryStream stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(stream, message);
            return stream.ToArray();
        }
        public static void Serialize(object message, Stream stream) {
            ProtoBuf.Serializer.Serialize(stream, message);
        }

        public static object Deserialize(Type type, Stream stream) {
            object o = ProtoBuf.Serializer.Deserialize(type, stream);
            if (o is ISupportInitialize supportInitialize)
            {
                supportInitialize.EndInit();
            }
            return o;
        }
    }
}