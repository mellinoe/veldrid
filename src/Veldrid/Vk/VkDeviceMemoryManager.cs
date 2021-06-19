using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Veldrid.Vk
{
    internal unsafe class VkDeviceMemoryManager : IDisposable
    {
        private readonly VkDevice _device;
        private readonly VkPhysicalDevice _physicalDevice;
        private readonly ulong _bufferImageGranularity;
        private readonly ulong _chunkGranularity;
        private readonly object _lock = new object();
        private ulong _totalAllocatedBytes;
        private readonly Dictionary<uint, ChunkAllocatorSet> _allocatorsByMemoryTypeUnmapped = new Dictionary<uint, ChunkAllocatorSet>();
        private readonly Dictionary<uint, ChunkAllocatorSet> _allocatorsByMemoryType = new Dictionary<uint, ChunkAllocatorSet>();

        private readonly vkGetBufferMemoryRequirements2_t _getBufferMemoryRequirements2;
        private readonly vkGetImageMemoryRequirements2_t _getImageMemoryRequirements2;

        public VkDeviceMemoryManager(
            VkDevice device,
            VkPhysicalDevice physicalDevice,
            ulong bufferImageGranularity,
            ulong chunkGranularity,
            vkGetBufferMemoryRequirements2_t getBufferMemoryRequirements2,
            vkGetImageMemoryRequirements2_t getImageMemoryRequirements2)
        {
            _device = device;
            _physicalDevice = physicalDevice;
            _bufferImageGranularity = bufferImageGranularity;
            _chunkGranularity = chunkGranularity;
            _getBufferMemoryRequirements2 = getBufferMemoryRequirements2;
            _getImageMemoryRequirements2 = getImageMemoryRequirements2;
        }

        public VkMemoryBlock Allocate(
            VkPhysicalDeviceMemoryProperties memProperties,
            uint memoryTypeBits,
            VkMemoryPropertyFlags flags,
            bool persistentMapped,
            ulong size,
            ulong alignment)
        {
            return Allocate(
                memProperties,
                memoryTypeBits,
                flags,
                persistentMapped,
                size,
                alignment,
                false,
                VkImage.Null,
                Vulkan.VkBuffer.Null);
        }

        public VkMemoryBlock Allocate(
            VkPhysicalDeviceMemoryProperties memProperties,
            uint memoryTypeBits,
            VkMemoryPropertyFlags flags,
            bool persistentMapped,
            ulong size,
            ulong alignment,
            bool dedicated,
            VkImage dedicatedImage,
            Vulkan.VkBuffer dedicatedBuffer)
        {
            lock (_lock)
            {
                if (!TryFindMemoryType(memProperties, memoryTypeBits, flags, out var memoryTypeIndex))
                {
                    throw new VeldridException("No suitable memory type.");
                }

                ulong minDedicatedAllocationSize = persistentMapped
                    ? ChunkAllocator.PersistentMappedChunkSize
                    : ChunkAllocator.UnmappedChunkSize;

                if (dedicated || size >= minDedicatedAllocationSize)
                {
                    // Round up to the nearest multiple of bufferImageGranularity.
                    size = ((size / _bufferImageGranularity) + 1) * _bufferImageGranularity;
                    _totalAllocatedBytes += size;

                    VkMemoryAllocateInfo allocateInfo = VkMemoryAllocateInfo.New();
                    allocateInfo.allocationSize = size;
                    allocateInfo.memoryTypeIndex = memoryTypeIndex;

                    VkMemoryDedicatedAllocateInfoKHR dedicatedAI;
                    if (dedicated)
                    {
                        dedicatedAI = VkMemoryDedicatedAllocateInfoKHR.New();
                        dedicatedAI.buffer = dedicatedBuffer;
                        dedicatedAI.image = dedicatedImage;
                        allocateInfo.pNext = &dedicatedAI;
                    }

                    VkResult allocationResult = vkAllocateMemory(_device, ref allocateInfo, null, out VkDeviceMemory memory);
                    if (allocationResult != VkResult.Success)
                    {
                        throw new VeldridException("Unable to allocate sufficient Vulkan memory.");
                    }

                    void* mappedPtr = null;
                    if (persistentMapped)
                    {
                        VkResult mapResult = vkMapMemory(_device, memory, 0, size, 0, &mappedPtr);
                        if (mapResult != VkResult.Success)
                        {
                            throw new VeldridException("Unable to map newly-allocated Vulkan memory.");
                        }
                    }

                    return new VkMemoryBlock(memory, 0, size, memoryTypeBits, mappedPtr, true);
                }
                else
                {
                    size = ((size / _chunkGranularity) + 1) * _chunkGranularity;
                    _totalAllocatedBytes += size;

                    ChunkAllocatorSet allocator = GetAllocator(memoryTypeIndex, persistentMapped);
                    bool result = allocator.Allocate(size, alignment, out VkMemoryBlock ret);
                    if (!result)
                    {
                        throw new VeldridException("Unable to allocate sufficient Vulkan memory.");
                    }

                    return ret;
                }
            }
        }

        public void Free(VkMemoryBlock block)
        {
            _totalAllocatedBytes -= block.Size;
            lock (_lock)
            {
                if (block.DedicatedAllocation)
                {
                    vkFreeMemory(_device, block.DeviceMemory, null);
                }
                else
                {
                    GetAllocator(block.MemoryTypeIndex, block.IsPersistentMapped).Free(block);
                }
            }
        }

        private ChunkAllocatorSet GetAllocator(uint memoryTypeIndex, bool persistentMapped)
        {
            ChunkAllocatorSet ret;

            if (persistentMapped)
            {
                if (!_allocatorsByMemoryType.TryGetValue(memoryTypeIndex, out ret))
                {
                    ret = new ChunkAllocatorSet(_device, memoryTypeIndex, true);
                    _allocatorsByMemoryType.Add(memoryTypeIndex, ret);
                }
            }
            else
            {
                if (!_allocatorsByMemoryTypeUnmapped.TryGetValue(memoryTypeIndex, out ret))
                {
                    ret = new ChunkAllocatorSet(_device, memoryTypeIndex, false);
                    _allocatorsByMemoryTypeUnmapped.Add(memoryTypeIndex, ret);
                }
            }

            return ret;
        }

        private class ChunkAllocatorSet : IDisposable
        {
            private readonly VkDevice _device;
            private readonly uint _memoryTypeIndex;
            private readonly bool _persistentMapped;
            private readonly List<ChunkAllocator> _allocators = new List<ChunkAllocator>();

            public ChunkAllocatorSet(VkDevice device, uint memoryTypeIndex, bool persistentMapped)
            {
                _device = device;
                _memoryTypeIndex = memoryTypeIndex;
                _persistentMapped = persistentMapped;
            }

            public bool Allocate(ulong size, ulong alignment, out VkMemoryBlock block)
            {
                for (int i = 0; i < _allocators.Count; i++)
                {
                    ChunkAllocator allocator = _allocators[i];
                    if (allocator.Allocate(size, alignment, out block))
                    {
                        return true;
                    }

                    // Allocate may merge free blocks.
                    if (allocator.IsFullFreeBlock())
                    {
                        allocator.Dispose();
                        _allocators.RemoveAt(i);
                        i--;
                    }
                }

                ChunkAllocator newAllocator = new ChunkAllocator(_device, _memoryTypeIndex, _persistentMapped);
                _allocators.Add(newAllocator);
                return newAllocator.Allocate(size, alignment, out block);
            }

            public void Free(VkMemoryBlock block)
            {
                for (int i = 0; i < _allocators.Count; i++)
                {
                    ChunkAllocator allocator = _allocators[i];
                    if (allocator.Memory == block.DeviceMemory)
                    {
                        allocator.Free(block);
                    }
                }
            }

            public void Dispose()
            {
                foreach (ChunkAllocator allocator in _allocators)
                {
                    allocator.Dispose();
                }
            }
        }

        private class ChunkAllocator : IDisposable
        {
            public const ulong PersistentMappedChunkSize = 1024 * 1024;
            public const ulong UnmappedChunkSize = 1024 * 1024 * 4;

            private readonly VkDevice _device;
            private readonly uint _memoryTypeIndex;
            private readonly bool _persistentMapped;
            private readonly List<VkMemoryBlock> _freeBlocks = new List<VkMemoryBlock>();
            private readonly VkDeviceMemory _memory;
            private readonly void* _mappedPtr;

            private ulong _totalMemorySize;

            public VkDeviceMemory Memory => _memory;

            public ChunkAllocator(VkDevice device, uint memoryTypeIndex, bool persistentMapped)
            {
                _device = device;
                _memoryTypeIndex = memoryTypeIndex;
                _persistentMapped = persistentMapped;
                _totalMemorySize = persistentMapped ? PersistentMappedChunkSize : UnmappedChunkSize;

                VkMemoryAllocateInfo memoryAI = VkMemoryAllocateInfo.New();
                memoryAI.allocationSize = _totalMemorySize;
                memoryAI.memoryTypeIndex = _memoryTypeIndex;
                VkResult result = vkAllocateMemory(_device, ref memoryAI, null, out _memory);
                CheckResult(result);

                void* mappedPtr = null;
                if (persistentMapped)
                {
                    result = vkMapMemory(_device, _memory, 0, _totalMemorySize, 0, &mappedPtr);
                    CheckResult(result);
                }
                _mappedPtr = mappedPtr;

                VkMemoryBlock initialBlock = new VkMemoryBlock(
                    _memory,
                    0,
                    _totalMemorySize,
                    _memoryTypeIndex,
                    _mappedPtr,
                    false);
                _freeBlocks.Add(initialBlock);
            }

            public bool Allocate(ulong size, ulong alignment, out VkMemoryBlock block)
            {
                checked
                {
                    List<VkMemoryBlock> freeBlocks = _freeBlocks;

                    // Don't try merging blocks if there are none.
                    bool hasMergedBlocks = freeBlocks.Count == 0;

                    do
                    {
                        for (int i = 0; i < freeBlocks.Count; i++)
                        {
                            VkMemoryBlock freeBlock = freeBlocks[i];
                            ulong alignedBlockSize = freeBlock.Size;
                            ulong alignedOffsetRemainder = freeBlock.Offset % alignment;
                            if (alignedOffsetRemainder != 0)
                            {
                                ulong alignmentCorrection = alignment - alignedOffsetRemainder;
                                if (alignedBlockSize <= alignmentCorrection)
                                {
                                    continue;
                                }
                                alignedBlockSize -= alignmentCorrection;
                            }

                            if (alignedBlockSize >= size) // Valid match -- split it and return.
                            {
                                block = freeBlock;
                                block.Size = alignedBlockSize;
                                if (alignedOffsetRemainder != 0)
                                {
                                    block.Offset += alignment - alignedOffsetRemainder;
                                }

                                if (alignedBlockSize != size)
                                {
                                    VkMemoryBlock splitBlock = new VkMemoryBlock(
                                        block.DeviceMemory,
                                        block.Offset + size,
                                        block.Size - size,
                                        _memoryTypeIndex,
                                        block.BaseMappedPointer,
                                        false);

                                    freeBlocks[i] = splitBlock;
                                    block.Size = size;
                                }
                                else
                                {
                                    freeBlocks.RemoveAt(i);
                                }

#if DEBUG
                                CheckAllocatedBlock(block);
#endif
                                return true;
                            }
                        }

                        if (hasMergedBlocks)
                        {
                            break;
                        }
                        hasMergedBlocks = MergeContiguousBlocks();
                    }
                    while (hasMergedBlocks);

                    block = default(VkMemoryBlock);
                    return false;
                }
            }

            private static int FindPrecedingBlockIndex(List<VkMemoryBlock> list, int length, ulong targetOffset)
            {
                int low = 0;
                int high = length - 1;

                if (length == 0 || list[high].Offset < targetOffset)
                    return -1;

                while (low <= high)
                {
                    int mid = low + ((high - low) / 2);

                    if (list[mid].Offset >= targetOffset)
                        high = mid - 1;
                    else
                        low = mid + 1;
                }

                return high + 1;
            }

            public void Free(VkMemoryBlock block)
            {
                // Assume that _freeBlocks is always sorted.
                int precedingBlock = FindPrecedingBlockIndex(_freeBlocks, _freeBlocks.Count, block.Offset);
                if (precedingBlock != -1)
                {
                    _freeBlocks.Insert(precedingBlock, block);
                }
                else
                {
                    _freeBlocks.Add(block);
                }

#if DEBUG
                RemoveAllocatedBlock(block);
#endif
            }

            private bool MergeContiguousBlocks()
            {
                List<VkMemoryBlock> freeBlocks = _freeBlocks;
                bool hasMerged = false;
                int contiguousLength = 1;

                for (int i = 0; i < freeBlocks.Count - 1; i++)
                {
                    ulong blockStart = freeBlocks[i].Offset;
                    while (i + contiguousLength < freeBlocks.Count
                        && freeBlocks[i + contiguousLength - 1].End == freeBlocks[i + contiguousLength].Offset)
                    {
                        contiguousLength += 1;
                    }

                    if (contiguousLength > 1)
                    {
                        ulong blockEnd = freeBlocks[i + contiguousLength - 1].End;
                        freeBlocks.RemoveRange(i, contiguousLength);

                        VkMemoryBlock mergedBlock = new VkMemoryBlock(
                            Memory,
                            blockStart,
                            blockEnd - blockStart,
                            _memoryTypeIndex,
                            _mappedPtr,
                            false);
                        freeBlocks.Insert(i, mergedBlock);
                        hasMerged = true;
                        contiguousLength = 0;
                    }
                }

                return hasMerged;
            }

#if DEBUG
            private HashSet<VkMemoryBlock> _allocatedBlocks = new HashSet<VkMemoryBlock>();

            private void CheckAllocatedBlock(VkMemoryBlock block)
            {
                foreach (VkMemoryBlock oldBlock in _allocatedBlocks)
                {
                    Debug.Assert(!BlocksOverlap(block, oldBlock), "Allocated blocks have overlapped.");
                }

                Debug.Assert(_allocatedBlocks.Add(block), "Same block added twice.");
            }

            private bool BlocksOverlap(VkMemoryBlock first, VkMemoryBlock second)
            {
                ulong firstStart = first.Offset;
                ulong firstEnd = first.Offset + first.Size;
                ulong secondStart = second.Offset;
                ulong secondEnd = second.Offset + second.Size;

                return (firstStart <= secondStart && firstEnd > secondStart
                    || firstStart >= secondStart && firstEnd <= secondEnd
                    || firstStart < secondEnd && firstEnd >= secondEnd
                    || firstStart <= secondStart && firstEnd >= secondEnd);
            }

            private void RemoveAllocatedBlock(VkMemoryBlock block)
            {
                Debug.Assert(_allocatedBlocks.Remove(block), "Unable to remove a supposedly allocated block.");
            }
#endif

            public bool IsFullFreeBlock()
            {
                if (_freeBlocks.Count == 1)
                {
                    VkMemoryBlock freeBlock = _freeBlocks[0];
                    return freeBlock.Offset == 0
                        && freeBlock.Size == _totalMemorySize;
                }
                return false;
            }

            public void Dispose()
            {
                vkFreeMemory(_device, _memory, null);
            }
        }

        public void Dispose()
        {
            foreach (KeyValuePair<uint, ChunkAllocatorSet> kvp in _allocatorsByMemoryType)
            {
                kvp.Value.Dispose();
            }

            foreach (KeyValuePair<uint, ChunkAllocatorSet> kvp in _allocatorsByMemoryTypeUnmapped)
            {
                kvp.Value.Dispose();
            }
        }

        internal IntPtr Map(VkMemoryBlock memoryBlock)
        {
            void* ret;
            VkResult result = vkMapMemory(_device, memoryBlock.DeviceMemory, memoryBlock.Offset, memoryBlock.Size, 0, &ret);
            CheckResult(result);
            return (IntPtr)ret;
        }
    }

    [DebuggerDisplay("[Mem:{DeviceMemory.Handle}] Off:{Offset}, Size:{Size} End:{Offset+Size}")]
    internal unsafe struct VkMemoryBlock : IEquatable<VkMemoryBlock>
    {
        public readonly uint MemoryTypeIndex;
        public readonly VkDeviceMemory DeviceMemory;
        public readonly void* BaseMappedPointer;
        public readonly bool DedicatedAllocation;

        public ulong Offset;
        public ulong Size;

        public void* BlockMappedPointer => ((byte*)BaseMappedPointer) + Offset;
        public bool IsPersistentMapped => BaseMappedPointer != null;
        public ulong End => Offset + Size;

        public VkMemoryBlock(
            VkDeviceMemory memory,
            ulong offset,
            ulong size,
            uint memoryTypeIndex,
            void* mappedPtr,
            bool dedicatedAllocation)
        {
            DeviceMemory = memory;
            Offset = offset;
            Size = size;
            MemoryTypeIndex = memoryTypeIndex;
            BaseMappedPointer = mappedPtr;
            DedicatedAllocation = dedicatedAllocation;
        }

        public bool Equals(VkMemoryBlock other)
        {
            return DeviceMemory.Equals(other.DeviceMemory)
                && Offset.Equals(other.Offset)
                && Size.Equals(other.Size);
        }
    }
}
