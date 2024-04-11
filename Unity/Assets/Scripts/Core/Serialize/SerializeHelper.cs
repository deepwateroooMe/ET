using System.IO;
using System;
namespace ET {
	// 双端静态、序列化与反序列化、静态帮助类。ProtoBuf 跨进程【序列化与反序列化】
	// 这里，感觉，ProtoBuf 里面，有飘渺峰上的飘渺逻辑，被亲爱的表哥的活宝妹，眼睁睁看、读源码、读丢了。。。
	// 这里，感觉，它的底层，有个有些什么【跨进程消息传递】之类的逻辑，细找一下
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