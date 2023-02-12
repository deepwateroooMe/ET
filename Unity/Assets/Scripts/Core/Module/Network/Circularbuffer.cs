using System;
using System.Collections.Generic;
using System.IO;
namespace ET {

    // 我记得上次把这个类读得狠懂了，怎么什么笔记也没有呢？没有保存吗？
    public class CircularBuffer: Stream { // 不是说，这是一条大河，高流量慢腾腾地流的传输方式吗？

        public int ChunkSize = 8192;
        private readonly Queue<byte[]> bufferQueue = new Queue<byte[]>(); // 当前操作队列
        private readonly Queue<byte[]> bufferCache = new Queue<byte[]>(); // 缓存队列
        public int LastIndex { get; set; }  // 块内的开始下标：不是大块的下标，仍然是以字节为单位的下标
        public int FirstIndex { get; set; } // 块内的结束下标：
        private byte[] lastBuffer;

        public CircularBuffer() {
            this.AddLast();
        }
        public override long Length {
            get {
                int c = 0;
                if (this.bufferQueue.Count == 0) {
                    c = 0;
                } else {
                    c = (this.bufferQueue.Count - 1) * ChunkSize + this.LastIndex - this.FirstIndex;
                }
                if (c < 0) {
                    Log.Error("CircularBuffer count < 0: {0}, {1}, {2}".Fmt(this.bufferQueue.Count, this.LastIndex, this.FirstIndex));
                }
                return c;
            }
        }
        public void AddLast() {
            byte[] buffer;
            if (this.bufferCache.Count > 0) {
                buffer = this.bufferCache.Dequeue();
            } else {
                buffer = new byte[ChunkSize];
            }
            this.bufferQueue.Enqueue(buffer);
            this.lastBuffer = buffer;
        }
        public void RemoveFirst() { // 不需要清理重置：是因为写数据的时候可以直接覆盖;没有被覆盖的部分也不会产生脏数据，是因为任何操作总是标明数据长短的，只要不错，就不会读出脏数据
            this.bufferCache.Enqueue(bufferQueue.Dequeue()); // 取出来，就直接回收缓存
        }
        public byte[] First {
            get {
                if (this.bufferQueue.Count == 0) {
                    this.AddLast();
                }
                return this.bufferQueue.Peek();
            }
        }
        public byte[] Last {
            get {
                if (this.bufferQueue.Count == 0) {
                    this.AddLast();
                }
                return this.lastBuffer;
            }
        }
        // 从CircularBuffer读到stream中
        // <param name="stream"></param>
        // public async ETTask ReadAsync(Stream stream)
        // {
        //    long buffLength = this.Length;
        //    int sendSize = this.ChunkSize - this.FirstIndex;
        //    if (sendSize > buffLength)
        //    {
        //        sendSize = (int)buffLength;
        //    }
        //    
        //    await stream.WriteAsync(this.First, this.FirstIndex, sendSize);
        //    
        //    this.FirstIndex += sendSize;
        //    if (this.FirstIndex == this.ChunkSize)
        //    {
        //        this.FirstIndex = 0;
        //        this.RemoveFirst();
        //    }
        // }
        // 从CircularBuffer读到stream(原标注)：以现结构缓冲区为中心的表达法
        // 想一想：当读写缓存区，独立于内存流之外，上下的过程是怎么样的？要写，就先写入缓存区，再由缓存区写入内存流. 要读，就从内存流先读入读缓存区，再作处理？
        public void Read(Stream stream, int count) { // 从缓存区中，读出固定的长度，并写入内存流中去
            if (count > this.Length) {
                throw new Exception($"bufferList length < count, {Length} {count}");
            }
            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count) {
                int n = count - alreadyCopyCount;
                if (ChunkSize - this.FirstIndex > n) {
                    stream.Write(this.First, this.FirstIndex, n);
                    this.FirstIndex += n;
                    alreadyCopyCount += n;
                } else {
                    stream.Write(this.First, this.FirstIndex, ChunkSize - this.FirstIndex);
                    alreadyCopyCount += ChunkSize - this.FirstIndex;
                    this.FirstIndex = 0;
                    this.RemoveFirst();
                }
            }
        }
        
        // 从stream写入CircularBuffer：从内存流写入缓存区
        public void Write(Stream stream) { 
            int count = (int)(stream.Length - stream.Position);
            
            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count) {
                if (this.LastIndex == ChunkSize) {
                    this.AddLast();
                    this.LastIndex = 0;
                }
                int n = count - alreadyCopyCount;
                if (ChunkSize - this.LastIndex > n) {
                    stream.Read(this.lastBuffer, this.LastIndex, n);
                    this.LastIndex += count - alreadyCopyCount;
                    alreadyCopyCount += n;
                } else {
                    stream.Read(this.lastBuffer, this.LastIndex, ChunkSize - this.LastIndex);
                    alreadyCopyCount += ChunkSize - this.LastIndex;
                    this.LastIndex = ChunkSize;
                }
            }
        }
        //  从stream写入CircularBuffer
        // <param name="stream"></param>
        // public async ETTask<int> WriteAsync(Stream stream)
        // {
        //    int size = this.ChunkSize - this.LastIndex;
        //    
        //    int n = await stream.ReadAsync(this.Last, this.LastIndex, size);
        //    if (n == 0)
        //    {
        //        return 0;
        //    }
        //    this.LastIndex += n;
        //    if (this.LastIndex == this.ChunkSize)
        //    {
        //        this.AddLast();
        //        this.LastIndex = 0;
        //    }
        //    return n;
        // }
        // 把CircularBuffer中数据写入buffer：把当前缓存区中的数据读出到传入的字节数组中
        public override int Read(byte[] buffer, int offset, int count) {
            if (buffer.Length < offset + count) {
                throw new Exception($"bufferList length < coutn, buffer length: {buffer.Length} {offset} {count}");
            }
            long length = this.Length;
            if (length < count) {
                count = (int)length;
            }
            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count) {
                int n = count - alreadyCopyCount;
                if (ChunkSize - this.FirstIndex > n) {
                    Array.Copy(this.First, this.FirstIndex, buffer, alreadyCopyCount + offset, n);
                    this.FirstIndex += n;
                    alreadyCopyCount += n;
                } else {
                    Array.Copy(this.First, this.FirstIndex, buffer, alreadyCopyCount + offset, ChunkSize - this.FirstIndex);
                    alreadyCopyCount += ChunkSize - this.FirstIndex;
                    this.FirstIndex = 0;
                    this.RemoveFirst();
                }
            }
            return count;
        }
        // 把buffer写入CircularBuffer中：把字节数组中的数据写入到字节数组中
        public override void Write(byte[] buffer, int offset, int count) {
            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count) {
                if (this.LastIndex == ChunkSize) {
                    this.AddLast();
                    this.LastIndex = 0;
                }
                int n = count - alreadyCopyCount;
                if (ChunkSize - this.LastIndex > n) {
                    Array.Copy(buffer, alreadyCopyCount + offset, this.lastBuffer, this.LastIndex, n);
                    this.LastIndex += count - alreadyCopyCount;
                    alreadyCopyCount += n;
                } else {
                    Array.Copy(buffer, alreadyCopyCount + offset, this.lastBuffer, this.LastIndex, ChunkSize - this.LastIndex);
                    alreadyCopyCount += ChunkSize - this.LastIndex;
                    this.LastIndex = ChunkSize;
                }
            }
        }
        public override void Flush() {
            throw new NotImplementedException();
        }
        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }
        public override void SetLength(long value) {
            throw new NotImplementedException();
        }
        public override bool CanRead {
            get {
                return true;
            }
        }
        public override bool CanSeek {
            get {
                return false;
            }
        }
        public override bool CanWrite {
            get {
                return true;
            }
        }
        public override long Position { get; set; }
    }
}

