using System;
using System.IO;
namespace ET {
    public static class MessageSerializeHelper {

        // 创建一个内存流：
        private static MemoryStream GetStream(int count = 0) {
            MemoryStream stream;
            if (count > 0) {
                stream = new MemoryStream(count); // 问系统要一个这么大小长度的，长度会固定不变，不可再扩展延伸，但系统分配的实际的 capacity 可能远不止这么长
            } else {
                stream = new MemoryStream(); // 使用默认无参数构造函数创建实例，可以使用Write方法写入，可延伸，随着字节数据的写入，数组的大小自动调整。
            }
            return stream;
        }

        // 把消息写入内存流的方法定义：这里主要是把这个消息的操作符写进去了
        public static (ushort, MemoryStream) MessageToStream(object message) {
            int headOffset = Packet.ActorIdLength; // 固定长度
            MemoryStream stream = GetStream(headOffset + Packet.OpcodeLength);  // 只要这么长，因为一般

            // 再底层一点儿，就搞不清楚：就是一个类型 Type, 就对应一个网络交流的操作符，类型不同，操作符一般不同？总之根据类型去拿操作符
            ushort opcode = NetServices.Instance.GetOpcode(message.GetType());
            stream.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin); // 使用seek方法定位读取器的当前的位置，可以通过指定长度的数组一次性读取指定长度的数据
            stream.SetLength(headOffset + Packet.OpcodeLength);
            stream.GetBuffer().WriteTo(headOffset, opcode); // 把操作符写进内存流里去
            SerializeHelper.Serialize(message, stream);     // 序列化消息：把要发送的消息序列化进内存流里，然后基本就可以不用管了，通信底层会处理剩下的？
            stream.Seek(0, SeekOrigin.Begin); // 重新把内存流的指针重置回0，底层发消息从0位开始发
            return (opcode, stream);
        }
    }
}