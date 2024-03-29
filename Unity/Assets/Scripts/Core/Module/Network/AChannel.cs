using System;
using System.IO;
using System.Net;
namespace ET {
    public enum ChannelType {
        Connect, // 客户端
        Accept,  // 服务端
    }
    public struct Packet { // 自定义、封装【信道上包裹、结构体】
        public const int MinPacketSize = 2;
        public const int OpcodeIndex = 8;
        public const int KcpOpcodeIndex = 0;
        public const int OpcodeLength = 2;
        public const int ActorIdIndex = 0;
        public const int ActorIdLength = 8;
        public const int MessageIndex = 10;
        public ushort Opcode;
        public long ActorId;
        public MemoryStream MemoryStream;
    }
    public abstract class AChannel: IDisposable {
        public long Id; // 身份证号
        public ChannelType ChannelType { get; protected set; } // 一个信道的双端，两端，分别拥有，不同的信道类型吗？
        public int Error { get; set; }
        public IPEndPoint RemoteAddress { get; set; } // 同样，一个信道的双端，两端，分别拥有，信道两端，各自的另一端远程端的地址吗？
        public bool IsDisposed {
            get {
                return this.Id == 0;    
            }
        }
        public abstract void Dispose();
    }
}