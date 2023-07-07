using System;
using System.IO;
namespace ET {
    public static class MessageSerializeHelper { // 这个类比较简单，底层大致扫了一下
        private static MemoryStream GetStream(int count = 0) {
            MemoryStream stream;
            if (count > 0) {
                stream = new MemoryStream(count);  // 指定长度：
            } else {
                stream = new MemoryStream(); // 默认长度
            }
            return stream;
        }
        // 定义：把消息序列化到内存流的逻辑。就是把相应的消息头写好，把消息写进内容体的的位置
        public static (ushort, MemoryStream) MessageToStream(object message) {
            int headOffset = Packet.ActorIdLength;
            MemoryStream stream = GetStream(headOffset + Packet.OpcodeLength); // 首先，分配指定长度的内存流
            ushort opcode = NetServices.Instance.GetOpcode(message.GetType()); // 获取消息的【网络操作符】
            stream.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin); // 快进到特定位置 
            stream.SetLength(headOffset + Packet.OpcodeLength); // 设置长度？
            stream.GetBuffer().WriteTo(headOffset, opcode);     // 指定的位置：写进网络操作符
            SerializeHelper.Serialize(message, stream);         // 序列化，到内存流。这里序列化的是消息 message 本身，【头儿】操作码什么的是不需要画蛇添足自找麻烦的
            stream.Seek(0, SeekOrigin.Begin); // 重置内存流指针到头上
            return (opcode, stream);          // 返回结构体包装体
        }
    }
}