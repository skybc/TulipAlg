using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TulipAlg.Core
{
    // SpanData
    /// <summary>
    /// Span<byte>对象池
    /// </summary>
    public class MemoryData : IDisposable
    {
        private bool _disposed = false;

        /// <summary>
        /// Span<byte>数据
        /// </summary>
        public IntPtr DataPointer
        {
            get; private set;
        }

        /// <summary>
        /// Span<byte>数据名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Span<byte>数据长度
        /// </summary>
        public int Length { get; private set; }

        private bool isLocal = true;



        /// <summary>
        /// 获取内存数据的Span表示
        /// </summary>
        /// <returns>表示内存数据的Span</returns>
        /// <exception cref="ObjectDisposedException">如果内存已被释放</exception>
        public unsafe Span<byte> GetData()
        {
            if (DataPointer == IntPtr.Zero)
            {
                return Span<byte>.Empty;
            }
            var re = new Span<byte>((void*)DataPointer, Length);
            return re;
        }
        /// <summary>
        /// 获取数据的Span表示，类型为T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public unsafe Span<T> GetData<T>() where T : unmanaged
        {
            if (DataPointer == IntPtr.Zero)
            {
                return Span<T>.Empty;
            }
            var re = new Span<T>((void*)DataPointer, Length / sizeof(T));
            return re;
        }

        public static implicit operator Span<byte>(MemoryData data)
        {
            return data.GetData();
        }



        /// <summary>
        /// 创建一个新的MemoryData实例
        /// </summary>
        /// <param name="data">指向内存的指针</param>
        /// <param name="name">内存数据的名称</param>
        /// <param name="length">内存长度</param>
        public MemoryData(IntPtr data, int length)
        {
            this.DataPointer = data;
            this.Length = length;
            this.isLocal = false;
        }


        public MemoryData(int length)
        {
            unsafe
            {
                this.DataPointer = (IntPtr)NativeMemory.AlignedAlloc((nuint)length, 4);
                this.Length = length;
            }
        }

        /// <summary>
        /// 实现IDisposable接口，释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否为显式调用</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                }

                // 释放非托管资源
                if (this.DataPointer != IntPtr.Zero)
                {
                    // 释放非托管内存
                    if (isLocal)
                    {
                        unsafe
                        {
                            NativeMemory.AlignedFree((void*)DataPointer);
                        }

                    }
                    this.DataPointer = IntPtr.Zero;
                    Length = 0;
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// 资源返回的终结器
        /// </summary>
        ~MemoryData()
        {
            Dispose(false);
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void Free()
        {
            if (DataPointer == IntPtr.Zero)
            {
                return;
            }
            Dispose();

        }

        /// <summary>
        /// 将byte数组复制到内存数据中
        /// </summary>
        /// <param name="data">要复制的数据</param>
        /// <exception cref="ArgumentOutOfRangeException">如果数据为null或长度超出分配的内存</exception>
        /// <exception cref="ObjectDisposedException">如果内存已被释放</exception>
        public void CopyFrom(byte[] data)
        {
            if (DataPointer == IntPtr.Zero)
            {
                throw new ObjectDisposedException(nameof(MemoryData), "内存已被释放或已返回到内存池。");
            }

            if (data == null || data.Length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "数据为 null 或超出分配的长度。");
            }
            //把数据复制到Span<byte>中 
            Marshal.Copy(data, 0, DataPointer, data.Length);
        }

        public void CopyFrom(Span<byte> data)
        {
            if (DataPointer == IntPtr.Zero)
            {
                throw new ObjectDisposedException(nameof(MemoryData), "内存已被释放或已返回到内存池。");
            }
            if (data.Length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "数据超出分配的长度。");
            }

            //把数据复制到Span<byte>中
            data.CopyTo(GetData());
        }

        public void CopyTo(Span<byte> data)
        {
            if (DataPointer == IntPtr.Zero)
            {
                throw new ObjectDisposedException(nameof(MemoryData), "内存已被释放或已返回到内存池。");
            }
            if (data.Length < Length)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "目标缓冲区没有足够的空间进行复制。");
            }

            //把数据复制到Span<byte>中
            GetData().CopyTo(data);
        }

        /// <summary>
        /// 将Span<byte>数据复制到byte[]数组中
        /// </summary>
        /// <param name="data">目标byte数组</param>
        /// <exception cref="ArgumentOutOfRangeException">如果数据为null或长度不足以容纳内存数据</exception>
        /// <exception cref="ObjectDisposedException">如果内存已被释放</exception>
        public void CopyTo(byte[] data)
        {
            if (DataPointer == IntPtr.Zero)
            {
                throw new ObjectDisposedException(nameof(MemoryData), "内存已被释放或已返回到内存池。");
            }

            if (data == null || data.Length < Length)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "目标数组为 null 或没有足够的空间进行复制。");
            }
            var span = GetData();
            span.CopyTo(data);
        }


    }

}
