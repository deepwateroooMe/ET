using System;
using System.IO;

namespace ET {

    // 它有两个阶段性状态：大小，数据
    public enum ParserState {
        PacketSize,
        PacketBody
    }

    public class PacketParser {
        private readonly CircularBuffer buffer;
        private int packetSize;
        private ParserState state;
        public AService service;
        private readonly byte[] cache = new byte[8];
        public const int InnerPacketSizeLength = 4; // 内网消息
        public const int OuterPacketSizeLength = 2; // 外网消息
        public MemoryStream MemoryStream;

        public PacketParser(CircularBuffer buffer, AService service) {
            this.buffer = buffer;
            this.service = service;
        }
        // 它也是分阶段来处理：第一步，处理消息头操作码;第二步，把读写缓存区中的内容，读进（写进）内存流，并把内存流的指针放置到正确的位置
        public bool Parse() {
            while (true) {
                switch (this.state) {
                case ParserState.PacketSize: {
                    if (this.service.ServiceType == ServiceType.Inner) {
                        if (this.buffer.Length < InnerPacketSizeLength) { // 内网消息，长度不对
                            return false;
                        }
                        this.buffer.Read(this.cache, 0, InnerPacketSizeLength);
                        this.packetSize = BitConverter.ToInt32(this.cache, 0);
                        if (this.packetSize > ushort.MaxValue * 16 || this.packetSize < Packet.MinPacketSize) {
                            throw new Exception($"recv packet size error, 可能是外网探测端口: {this.packetSize}");
                        }
                    } else {
                        if (this.buffer.Length < OuterPacketSizeLength) { // 外网消息，长度不对
                            return false;
                        }
                        this.buffer.Read(this.cache, 0, OuterPacketSizeLength);
                        this.packetSize = BitConverter.ToUInt16(this.cache, 0);
                        if (this.packetSize < Packet.MinPacketSize) {
                            throw new Exception($"recv packet size error, 可能是外网探测端口: {this.packetSize}");
                        }
                    }
                    this.state = ParserState.PacketBody;
                    break;
                }
                    // 第二步，把读写缓存区中的内容，读进（写进）内存流，并把内存流的指针放置到正确的位置
                case ParserState.PacketBody: {
                    if (this.buffer.Length < this.packetSize) {
                        return false;
                    }
                    MemoryStream memoryStream = new MemoryStream(this.packetSize);
                    this.buffer.Read(memoryStream, this.packetSize);
                    // memoryStream.SetLength(this.packetSize - Packet.MessageIndex);
                    this.MemoryStream = memoryStream;
                    if (this.service.ServiceType == ServiceType.Inner) {
                        memoryStream.Seek(Packet.MessageIndex, SeekOrigin.Begin);
                    } else {
                        memoryStream.Seek(Packet.OpcodeLength, SeekOrigin.Begin);
                    }
                    this.state = ParserState.PacketSize; // 重置状态
                    return true;
                }
                default:
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}