using System;
using System.IO;
using System.Net;

namespace ET {

    public enum ChannelType { // 这里是说，信道的连接方向吗？一种是专用来连接的，一种是专用来接受数据的
        Connect,
        Accept,
    }

    // 定义的是传输过程中的一个包裹： 这，可以是公认的，也可以是自定义的协议规范 ？有客户端与服务器们之间公认的协议格式
    public struct Packet {

        public const int MinPacketSize = 2;

        public const int OpcodeIndex = 8;
        public const int KcpOpcodeIndex = 0;
        public const int OpcodeLength = 2;

        public const int ActorIdIndex = 0;
        public const int ActorIdLength = 8;
        public const int MessageIndex = 10;

        public ushort Opcode; // 操作码
        public long ActorId;
        public MemoryStream MemoryStream; // 内存流
    }

    public abstract class AChannel: IDisposable {
        public long Id;
        
        public ChannelType ChannelType { get; protected set; }
        public int Error { get; set; }
        
        public IPEndPoint RemoteAddress { get; set; }
        
        public bool IsDisposed {
            get {
                return this.Id == 0;    
            }
        }
        public abstract void Dispose();
    }
}