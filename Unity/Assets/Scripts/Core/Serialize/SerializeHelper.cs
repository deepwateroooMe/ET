using System.IO;
using System;
namespace ET {
    // 进程间序列化帮助类：因为框架进程间的序列化、反序列化选择使用的是 Protobuf, 所以帮助类全借助这个工具来实现。
    // 那么，序列化到数据库，属于不同的使用场景吗？数据库不该也是应该会是在另一个进程，进程间序与反，什么时候使用的是 Bson ？
    public static class SerializeHelper {
        public static object Deserialize(Type type, byte[] bytes, int index, int count) {
            return ProtobufHelper.Deserialize(type, bytes, index, count);
        }
        public static byte[] Serialize(object message) {
            return ProtobufHelper.Serialize(message);
        }
        public static void Serialize(object message, Stream stream) {
            ProtobufHelper.Serialize(message, stream);
        }
        public static object Deserialize(Type type, Stream stream) {
            return ProtobufHelper.Deserialize(type, stream);
        }
    }
}