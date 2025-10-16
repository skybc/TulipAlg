using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TulipAlg.Core
{ 
    public class MemoryDataFloat : IDisposable
    {
        private bool _disposed = false;

        /// <summary>
        /// Span<byte>数据
        /// </summary>
        public IntPtr DataPointer
        {
            get; private set;
        }
        public int Length { get; private set; }

        public MemoryDataFloat(int length)
        {
            unsafe
            {
                this.DataPointer = (IntPtr)NativeMemory.AlignedAlloc((nuint)(length * 4), 4);
                this.Length = length;
            }
        }

        // 转换为Span<float>
        public static implicit operator Span<float>(MemoryDataFloat data)
        {
            return data.GetData();
        }


        public unsafe Span<float> GetData()
        {
            if (DataPointer == IntPtr.Zero)
            {
                return Span<float>.Empty;
            }
            var re = new Span<float>((void*)DataPointer, Length);
            return re;
        }

        ~MemoryDataFloat()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool v)
        {
            if (!_disposed)
            {
                if (DataPointer != IntPtr.Zero)
                {
                    unsafe
                    {
                        NativeMemory.AlignedFree((void*)DataPointer);
                    }
                    DataPointer = IntPtr.Zero;
                    Length = 0;
                }
            }
        }
    }

}
