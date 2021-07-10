using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Veldrid.Vk
{
    /// <summary>
    /// A super-dangerous stack-only list which can hold up to 256 bytes of blittable data.
    /// </summary>
    /// <typeparam name="T">The type of element held in the list. Must be blittable.</typeparam>
    internal unsafe struct StackList<T> where T : struct
    {
        public const int CapacityInBytes = 256;

        private fixed byte _storage[CapacityInBytes];
        private uint _count;

        public uint Count => _count;
        public void* Data => Unsafe.AsPointer(ref this);

        public void Add(T item)
        {
            byte* basePtr = (byte*)Data;
            int offset = (int)(_count * Unsafe.SizeOf<T>());
#if DEBUG
            Debug.Assert((offset + Unsafe.SizeOf<T>()) <= CapacityInBytes);
#endif
            Unsafe.Write(basePtr + offset, item);

            _count += 1;
        }

        public ref T this[uint index]
        {
            get
            {
                byte* basePtr = (byte*)Unsafe.AsPointer(ref this);
                int offset = (int)(index * Unsafe.SizeOf<T>());
                return ref Unsafe.AsRef<T>(basePtr + offset);
            }
        }

        public ref T this[int index]
        {
            get
            {
                byte* basePtr = (byte*)Unsafe.AsPointer(ref this);
                int offset = index * Unsafe.SizeOf<T>();
                return ref Unsafe.AsRef<T>(basePtr + offset);
            }
        }
    }

    /// <summary>
    /// A super-dangerous stack-only list which can hold a number of bytes determined by the second type parameter.
    /// </summary>
    /// <typeparam name="T">The type of element held in the list. Must be blittable.</typeparam>
    /// <typeparam name="TSize">A type parameter dictating the capacity of the list.</typeparam>
    internal unsafe struct StackList<T, TSize> where T : struct where TSize : struct
    {
#pragma warning disable 0169 // Unused field. This is used implicity because it controls the size of the structure on the stack.
        private TSize _storage;
#pragma warning restore 0169
        private uint _count;

        public uint Count => _count;
        public void* Data => Unsafe.AsPointer(ref this);

        public void Add(T item)
        {
            ref T dest = ref Unsafe.Add(ref Unsafe.As<TSize, T>(ref _storage), (int)_count);
#if DEBUG
            int offset = (int)(_count * Unsafe.SizeOf<T>());
            Debug.Assert((offset + Unsafe.SizeOf<T>()) <= Unsafe.SizeOf<TSize>());
#endif
            dest = item;

            _count += 1;
        }

        public ref T this[int index] => ref Unsafe.Add(ref Unsafe.AsRef<T>(Data), index);
        public ref T this[uint index] => ref Unsafe.Add(ref Unsafe.AsRef<T>(Data), (int)index);
    }

    internal unsafe struct Size16Bytes { public fixed byte Data[16]; }
    internal unsafe struct Size64Bytes { public fixed byte Data[64]; }
    internal unsafe struct Size128Bytes { public fixed byte Data[64]; }
    internal unsafe struct Size512Bytes { public fixed byte Data[1024]; }
    internal unsafe struct Size1024Bytes { public fixed byte Data[1024]; }
    internal unsafe struct Size2048Bytes { public fixed byte Data[2048]; }
#pragma warning disable 0649 // Fields are not assigned directly -- expected.
    internal unsafe struct Size2IntPtr { public IntPtr First; public IntPtr Second; }
    internal unsafe struct Size6IntPtr { public IntPtr First; public IntPtr Second; public IntPtr Third; public IntPtr Fourth; public IntPtr Fifth; public IntPtr Sixth; }
#pragma warning restore 0649
}
