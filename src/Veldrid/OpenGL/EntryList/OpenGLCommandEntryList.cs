using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.OpenGL.EntryList
{
    internal unsafe class OpenGLCommandEntryList : IDisposable
    {
        private readonly StagingMemoryPool _memoryPool;
        private readonly List<EntryStorageBlock> _blocks = new List<EntryStorageBlock>();
        private EntryStorageBlock _currentBlock;
        private uint _totalEntries;
        private readonly List<object> _resourceList = new List<object>();
        private readonly List<StagingBlock> _stagingBlocks = new List<StagingBlock>();

        // Entry IDs
        private const byte BeginEntryID = 1;
        private const byte ClearColorTargetID = 2;
        private const byte ClearDepthTargetID = 3;
        private const byte DrawIndexedEntryID = 4;
        private const byte EndEntryID = 5;
        private const byte SetFramebufferEntryID = 6;
        private const byte SetIndexBufferEntryID = 7;
        private const byte SetPipelineEntryID = 8;
        private const byte SetResourceSetEntryID = 9;
        private const byte SetScissorRectEntryID = 10;
        private const byte SetVertexBufferEntryID = 11;
        private const byte SetViewportEntryID = 12;
        private const byte UpdateBufferEntryID = 13;
        private const byte CopyBufferEntryID = 14;
        private const byte CopyTextureEntryID = 15;
        private const byte ResolveTextureEntryID = 16;
        private const byte DrawEntryID = 17;
        private const byte DispatchEntryID = 18;
        private const byte DrawIndirectEntryID = 20;
        private const byte DrawIndexedIndirectEntryID = 21;
        private const byte DispatchIndirectEntryID = 22;
        private const byte GenerateMipmapsEntryID = 23;
        private const byte PushDebugGroupEntryID = 24;
        private const byte PopDebugGroupEntryID = 25;
        private const byte InsertDebugMarkerEntryID = 26;

        public OpenGLCommandList Parent { get; }

        public OpenGLCommandEntryList(OpenGLCommandList cl)
        {
            Parent = cl;
            _memoryPool = cl.Device.StagingMemoryPool;
            _currentBlock = EntryStorageBlock.New();
            _blocks.Add(_currentBlock);
        }

        public void Reset()
        {
            FlushStagingBlocks();
            _resourceList.Clear();
            _totalEntries = 0;
            _currentBlock = _blocks[0];
            foreach (EntryStorageBlock block in _blocks)
            {
                block.Clear();
            }
        }

        public void Dispose()
        {
            FlushStagingBlocks();
            _resourceList.Clear();
            _totalEntries = 0;
            _currentBlock = _blocks[0];
            foreach (EntryStorageBlock block in _blocks)
            {
                block.Clear();
                block.Free();
            }
        }

        private void FlushStagingBlocks()
        {
            StagingMemoryPool pool = _memoryPool;
            foreach (StagingBlock block in _stagingBlocks)
            {
                pool.Free(block);
            }

            _stagingBlocks.Clear();
        }

        public void* GetStorageChunk(uint size, out byte* terminatorWritePtr)
        {
            terminatorWritePtr = null;
            if (!_currentBlock.Alloc(size, out void* ptr))
            {
                int currentBlockIndex = _blocks.IndexOf(_currentBlock);
                bool anyWorked = false;
                for (int i = currentBlockIndex + 1; i < _blocks.Count; i++)
                {
                    EntryStorageBlock nextBlock = _blocks[i];
                    if (nextBlock.Alloc(size, out ptr))
                    {
                        _currentBlock = nextBlock;
                        anyWorked = true;
                        break;
                    }
                }

                if (!anyWorked)
                {
                    _currentBlock = EntryStorageBlock.New();
                    _blocks.Add(_currentBlock);
                    bool result = _currentBlock.Alloc(size, out ptr);
                    Debug.Assert(result);
                }
            }
            if (_currentBlock.RemainingSize > size)
            {
                terminatorWritePtr = (byte*)ptr + size;
            }

            return ptr;
        }

        public void AddEntry<T>(byte id, ref T entry) where T : unmanaged
        {
            uint storageSize = (uint)Unsafe.SizeOf<T>() + 1; // Include ID
            void* storagePtr = GetStorageChunk(storageSize, out byte* terminatorWritePtr);
            Unsafe.Write(storagePtr, id);
            Unsafe.Write((byte*)storagePtr + 1, entry);
            if (terminatorWritePtr != null)
            {
                *terminatorWritePtr = 0;
            }
            _totalEntries += 1;
        }

        public void ExecuteAll(OpenGLCommandExecutor executor)
        {
            int currentBlockIndex = 0;
            EntryStorageBlock block = _blocks[currentBlockIndex];
            uint currentOffset = 0;
            for (uint i = 0; i < _totalEntries; i++)
            {
                if (currentOffset == block.TotalSize)
                {
                    currentBlockIndex += 1;
                    block = _blocks[currentBlockIndex];
                    currentOffset = 0;
                }

                uint id = Unsafe.Read<byte>(block.BasePtr + currentOffset);
                if (id == 0)
                {
                    currentBlockIndex += 1;
                    block = _blocks[currentBlockIndex];
                    currentOffset = 0;
                    id = Unsafe.Read<byte>(block.BasePtr + currentOffset);
                }

                Debug.Assert(id != 0);
                currentOffset += 1;
                byte* entryBasePtr = block.BasePtr + currentOffset;
                switch (id)
                {
                    case BeginEntryID:
                        executor.Begin();
                        currentOffset += (uint)Unsafe.SizeOf<BeginEntry>();
                        break;
                    case ClearColorTargetID:
                        ClearColorTargetEntry ccte = Unsafe.ReadUnaligned<ClearColorTargetEntry>(entryBasePtr);
                        executor.ClearColorTarget(ccte.Index, ccte.ClearColor);
                        currentOffset += (uint)Unsafe.SizeOf<ClearColorTargetEntry>();
                        break;
                    case ClearDepthTargetID:
                        ClearDepthTargetEntry cdte = Unsafe.ReadUnaligned<ClearDepthTargetEntry>(entryBasePtr);
                        executor.ClearDepthStencil(cdte.Depth, cdte.Stencil);
                        currentOffset += (uint)Unsafe.SizeOf<ClearDepthTargetEntry>();
                        break;
                    case DrawEntryID:
                        DrawEntry de = Unsafe.ReadUnaligned<DrawEntry>(entryBasePtr);
                        executor.Draw(de.VertexCount, de.InstanceCount, de.VertexStart, de.InstanceStart);
                        currentOffset += (uint)Unsafe.SizeOf<DrawEntry>();
                        break;
                    case DrawIndexedEntryID:
                        DrawIndexedEntry die = Unsafe.ReadUnaligned<DrawIndexedEntry>(entryBasePtr);
                        executor.DrawIndexed(die.IndexCount, die.InstanceCount, die.IndexStart, die.VertexOffset, die.InstanceStart);
                        currentOffset += (uint)Unsafe.SizeOf<DrawIndexedEntry>();
                        break;
                    case DrawIndirectEntryID:
                        DrawIndirectEntry drawIndirectEntry = Unsafe.ReadUnaligned<DrawIndirectEntry>(entryBasePtr);
                        executor.DrawIndirect(
                            drawIndirectEntry.IndirectBuffer.Get(_resourceList),
                            drawIndirectEntry.Offset,
                            drawIndirectEntry.DrawCount,
                            drawIndirectEntry.Stride);
                        currentOffset += (uint)Unsafe.SizeOf<DrawIndirectEntry>();
                        break;
                    case DrawIndexedIndirectEntryID:
                        DrawIndexedIndirectEntry diie = Unsafe.ReadUnaligned<DrawIndexedIndirectEntry>(entryBasePtr);
                        executor.DrawIndexedIndirect(diie.IndirectBuffer.Get(_resourceList), diie.Offset, diie.DrawCount, diie.Stride);
                        currentOffset += (uint)Unsafe.SizeOf<DrawIndexedIndirectEntry>();
                        break;
                    case DispatchEntryID:
                        DispatchEntry dispatchEntry = Unsafe.ReadUnaligned<DispatchEntry>(entryBasePtr);
                        executor.Dispatch(dispatchEntry.GroupCountX, dispatchEntry.GroupCountY, dispatchEntry.GroupCountZ);
                        currentOffset += (uint)Unsafe.SizeOf<DispatchEntry>();
                        break;
                    case DispatchIndirectEntryID:
                        DispatchIndirectEntry dispatchIndir = Unsafe.ReadUnaligned<DispatchIndirectEntry>(entryBasePtr);
                        executor.DispatchIndirect(dispatchIndir.IndirectBuffer.Get(_resourceList), dispatchIndir.Offset);
                        currentOffset += (uint)Unsafe.SizeOf<DispatchIndirectEntry>();
                        break;
                    case EndEntryID:
                        executor.End();
                        currentOffset += (uint)Unsafe.SizeOf<EndEntry>();
                        break;
                    case SetFramebufferEntryID:
                        SetFramebufferEntry sfbe = Unsafe.ReadUnaligned<SetFramebufferEntry>(entryBasePtr);
                        executor.SetFramebuffer(sfbe.Framebuffer.Get(_resourceList));
                        currentOffset += (uint)Unsafe.SizeOf<SetFramebufferEntry>();
                        break;
                    case SetIndexBufferEntryID:
                        SetIndexBufferEntry sibe = Unsafe.ReadUnaligned<SetIndexBufferEntry>(entryBasePtr);
                        executor.SetIndexBuffer(sibe.Buffer.Get(_resourceList), sibe.Format, sibe.Offset);
                        currentOffset += (uint)Unsafe.SizeOf<SetIndexBufferEntry>();
                        break;
                    case SetPipelineEntryID:
                        SetPipelineEntry spe = Unsafe.ReadUnaligned<SetPipelineEntry>(entryBasePtr);
                        executor.SetPipeline(spe.Pipeline.Get(_resourceList));
                        currentOffset += (uint)Unsafe.SizeOf<SetPipelineEntry>();
                        break;
                    case SetResourceSetEntryID:
                        SetResourceSetEntry srse = Unsafe.ReadUnaligned<SetResourceSetEntry>(entryBasePtr);
                        ResourceSet rs = srse.ResourceSet.Get(_resourceList);
                        uint* dynamicOffsetsPtr = srse.DynamicOffsetCount > SetResourceSetEntry.MaxInlineDynamicOffsets
                            ? (uint*)srse.DynamicOffsets_Block.Data
                            : srse.DynamicOffsets_Inline;
                        if (srse.IsGraphics)
                        {
                            executor.SetGraphicsResourceSet(
                                srse.Slot,
                                rs,
                                srse.DynamicOffsetCount,
                                ref Unsafe.AsRef<uint>(dynamicOffsetsPtr));
                        }
                        else
                        {
                            executor.SetComputeResourceSet(
                                srse.Slot,
                                rs,
                                srse.DynamicOffsetCount,
                                ref Unsafe.AsRef<uint>(dynamicOffsetsPtr));
                        }
                        currentOffset += (uint)Unsafe.SizeOf<SetResourceSetEntry>();
                        break;
                    case SetScissorRectEntryID:
                        SetScissorRectEntry ssre = Unsafe.ReadUnaligned<SetScissorRectEntry>(entryBasePtr);
                        executor.SetScissorRect(ssre.Index, ssre.X, ssre.Y, ssre.Width, ssre.Height);
                        currentOffset += (uint)Unsafe.SizeOf<SetScissorRectEntry>();
                        break;
                    case SetVertexBufferEntryID:
                        SetVertexBufferEntry svbe = Unsafe.ReadUnaligned<SetVertexBufferEntry>(entryBasePtr);
                        executor.SetVertexBuffer(svbe.Index, svbe.Buffer.Get(_resourceList), svbe.Offset);
                        currentOffset += (uint)Unsafe.SizeOf<SetVertexBufferEntry>();
                        break;
                    case SetViewportEntryID:
                        SetViewportEntry svpe = Unsafe.ReadUnaligned<SetViewportEntry>(entryBasePtr);
                        executor.SetViewport(svpe.Index, ref svpe.Viewport);
                        currentOffset += (uint)Unsafe.SizeOf<SetViewportEntry>();
                        break;
                    case UpdateBufferEntryID:
                        UpdateBufferEntry ube = Unsafe.ReadUnaligned<UpdateBufferEntry>(entryBasePtr);
                        byte* dataPtr = (byte*)ube.StagingBlock.Data;
                        executor.UpdateBuffer(
                            ube.Buffer.Get(_resourceList),
                            ube.BufferOffsetInBytes,
                            (IntPtr)dataPtr, ube.StagingBlockSize);
                        currentOffset += (uint)Unsafe.SizeOf<UpdateBufferEntry>();
                        break;
                    case CopyBufferEntryID:
                        CopyBufferEntry cbe = Unsafe.ReadUnaligned<CopyBufferEntry>(entryBasePtr);
                        executor.CopyBuffer(
                            cbe.Source.Get(_resourceList),
                            cbe.SourceOffset,
                            cbe.Destination.Get(_resourceList),
                            cbe.DestinationOffset,
                            cbe.SizeInBytes);
                        currentOffset += (uint)Unsafe.SizeOf<CopyBufferEntry>();
                        break;
                    case CopyTextureEntryID:
                        CopyTextureEntry cte = Unsafe.ReadUnaligned<CopyTextureEntry>(entryBasePtr);
                        executor.CopyTexture(
                            cte.Source.Get(_resourceList),
                            cte.SrcX, cte.SrcY, cte.SrcZ,
                            cte.SrcMipLevel,
                            cte.SrcBaseArrayLayer,
                            cte.Destination.Get(_resourceList),
                            cte.DstX, cte.DstY, cte.DstZ,
                            cte.DstMipLevel,
                            cte.DstBaseArrayLayer,
                            cte.Width, cte.Height, cte.Depth,
                            cte.LayerCount);
                        currentOffset += (uint)Unsafe.SizeOf<CopyTextureEntry>();
                        break;
                    case ResolveTextureEntryID:
                        ResolveTextureEntry rte = Unsafe.ReadUnaligned<ResolveTextureEntry>(entryBasePtr);
                        executor.ResolveTexture(rte.Source.Get(_resourceList), rte.Destination.Get(_resourceList));
                        currentOffset += (uint)Unsafe.SizeOf<ResolveTextureEntry>();
                        break;
                    case GenerateMipmapsEntryID:
                        GenerateMipmapsEntry gme = Unsafe.ReadUnaligned<GenerateMipmapsEntry>(entryBasePtr);
                        executor.GenerateMipmaps(gme.Texture.Get(_resourceList));
                        currentOffset += (uint)Unsafe.SizeOf<GenerateMipmapsEntry>();
                        break;
                    case PushDebugGroupEntryID:
                        PushDebugGroupEntry pdge = Unsafe.ReadUnaligned<PushDebugGroupEntry>(entryBasePtr);
                        executor.PushDebugGroup(pdge.Name.Get(_resourceList));
                        currentOffset += (uint)Unsafe.SizeOf<PushDebugGroupEntry>();
                        break;
                    case PopDebugGroupEntryID:
                        executor.PopDebugGroup();
                        currentOffset += (uint)Unsafe.SizeOf<PopDebugGroupEntry>();
                        break;
                    case InsertDebugMarkerEntryID:
                        InsertDebugMarkerEntry idme = Unsafe.ReadUnaligned<InsertDebugMarkerEntry>(entryBasePtr);
                        executor.InsertDebugMarker(idme.Name.Get(_resourceList));
                        currentOffset += (uint)Unsafe.SizeOf<InsertDebugMarkerEntry>();
                        break;
                    default:
                        throw new InvalidOperationException("Invalid entry ID: " + id);
                }
            }
        }

        public void Begin()
        {
            BeginEntry entry = new BeginEntry();
            AddEntry(BeginEntryID, ref entry);
        }

        public void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            ClearColorTargetEntry entry = new ClearColorTargetEntry(index, clearColor);
            AddEntry(ClearColorTargetID, ref entry);
        }

        public void ClearDepthTarget(float depth, byte stencil)
        {
            ClearDepthTargetEntry entry = new ClearDepthTargetEntry(depth, stencil);
            AddEntry(ClearDepthTargetID, ref entry);
        }

        public void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            DrawEntry entry = new DrawEntry(vertexCount, instanceCount, vertexStart, instanceStart);
            AddEntry(DrawEntryID, ref entry);
        }

        public void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            DrawIndexedEntry entry = new DrawIndexedEntry(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
            AddEntry(DrawIndexedEntryID, ref entry);
        }

        public void DrawIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            DrawIndirectEntry entry = new DrawIndirectEntry(Track(indirectBuffer), offset, drawCount, stride);
            AddEntry(DrawIndirectEntryID, ref entry);
        }

        public void DrawIndexedIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            DrawIndexedIndirectEntry entry = new DrawIndexedIndirectEntry(Track(indirectBuffer), offset, drawCount, stride);
            AddEntry(DrawIndexedIndirectEntryID, ref entry);
        }

        public void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            DispatchEntry entry = new DispatchEntry(groupCountX, groupCountY, groupCountZ);
            AddEntry(DispatchEntryID, ref entry);
        }

        public void DispatchIndirect(DeviceBuffer indirectBuffer, uint offset)
        {
            DispatchIndirectEntry entry = new DispatchIndirectEntry(Track(indirectBuffer), offset);
            AddEntry(DispatchIndirectEntryID, ref entry);
        }

        public void End()
        {
            EndEntry entry = new EndEntry();
            AddEntry(EndEntryID, ref entry);
        }

        public void SetFramebuffer(Framebuffer fb)
        {
            SetFramebufferEntry entry = new SetFramebufferEntry(Track(fb));
            AddEntry(SetFramebufferEntryID, ref entry);
        }

        public void SetIndexBuffer(DeviceBuffer buffer, IndexFormat format, uint offset)
        {
            SetIndexBufferEntry entry = new SetIndexBufferEntry(Track(buffer), format, offset);
            AddEntry(SetIndexBufferEntryID, ref entry);
        }

        public void SetPipeline(Pipeline pipeline)
        {
            SetPipelineEntry entry = new SetPipelineEntry(Track(pipeline));
            AddEntry(SetPipelineEntryID, ref entry);
        }

        public void SetGraphicsResourceSet(uint slot, ResourceSet rs, ReadOnlySpan<uint> dynamicOffsets)
        {
            SetResourceSet(slot, rs, dynamicOffsets, isGraphics: true);
        }

        private void SetResourceSet(uint slot, ResourceSet rs, ReadOnlySpan<uint> dynamicOffsets, bool isGraphics)
        {
            SetResourceSetEntry entry;

            if (dynamicOffsets.Length > SetResourceSetEntry.MaxInlineDynamicOffsets)
            {
                StagingBlock block = _memoryPool.GetStagingBlock((uint)dynamicOffsets.Length * sizeof(uint));
                _stagingBlocks.Add(block);
                for (int i = 0; i < dynamicOffsets.Length; i++)
                {
                    ((uint*)block.Data)[i] = dynamicOffsets[i];
                }

                entry = new SetResourceSetEntry(slot, Track(rs), isGraphics, block);
            }
            else
            {
                entry = new SetResourceSetEntry(slot, Track(rs), isGraphics, dynamicOffsets);
            }

            AddEntry(SetResourceSetEntryID, ref entry);
        }

        public void SetComputeResourceSet(uint slot, ResourceSet rs, ReadOnlySpan<uint> dynamicOffsets)
        {
            SetResourceSet(slot, rs, dynamicOffsets, isGraphics: false);
        }

        public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            SetScissorRectEntry entry = new SetScissorRectEntry(index, x, y, width, height);
            AddEntry(SetScissorRectEntryID, ref entry);
        }

        public void SetVertexBuffer(uint index, DeviceBuffer buffer, uint offset)
        {
            SetVertexBufferEntry entry = new SetVertexBufferEntry(index, Track(buffer), offset);
            AddEntry(SetVertexBufferEntryID, ref entry);
        }

        public void SetViewport(uint index, ref Viewport viewport)
        {
            SetViewportEntry entry = new SetViewportEntry(index, ref viewport);
            AddEntry(SetViewportEntryID, ref entry);
        }

        public void ResolveTexture(Texture source, Texture destination)
        {
            ResolveTextureEntry entry = new ResolveTextureEntry(Track(source), Track(destination));
            AddEntry(ResolveTextureEntryID, ref entry);
        }

        public void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            StagingBlock stagingBlock = _memoryPool.Stage(source, sizeInBytes);
            _stagingBlocks.Add(stagingBlock);
            UpdateBufferEntry entry = new UpdateBufferEntry(Track(buffer), bufferOffsetInBytes, stagingBlock, sizeInBytes);
            AddEntry(UpdateBufferEntryID, ref entry);
        }

        public void CopyBuffer(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
        {
            CopyBufferEntry entry = new CopyBufferEntry(
                Track(source),
                sourceOffset,
                Track(destination),
                destinationOffset,
                sizeInBytes);
            AddEntry(CopyBufferEntryID, ref entry);
        }

        public void CopyTexture(
            Texture source,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel,
            uint srcBaseArrayLayer,
            Texture destination,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel,
            uint dstBaseArrayLayer,
            uint width, uint height, uint depth,
            uint layerCount)
        {
            CopyTextureEntry entry = new CopyTextureEntry(
                Track(source),
                srcX, srcY, srcZ,
                srcMipLevel,
                srcBaseArrayLayer,
                Track(destination),
                dstX, dstY, dstZ,
                dstMipLevel,
                dstBaseArrayLayer,
                width, height, depth,
                layerCount);
            AddEntry(CopyTextureEntryID, ref entry);
        }

        public void GenerateMipmaps(Texture texture)
        {
            GenerateMipmapsEntry entry = new GenerateMipmapsEntry(Track(texture));
            AddEntry(GenerateMipmapsEntryID, ref entry);
        }

        public void PushDebugGroup(string name)
        {
            PushDebugGroupEntry entry = new PushDebugGroupEntry(Track(name));
            AddEntry(PushDebugGroupEntryID, ref entry);
        }

        public void PopDebugGroup()
        {
            PopDebugGroupEntry entry = new PopDebugGroupEntry();
            AddEntry(PopDebugGroupEntryID, ref entry);
        }

        public void InsertDebugMarker(string name)
        {
            InsertDebugMarkerEntry entry = new InsertDebugMarkerEntry(Track(name));
            AddEntry(InsertDebugMarkerEntryID, ref entry);
        }

        private Tracked<T> Track<T>(T item) where T : class
        {
            return new Tracked<T>(_resourceList, item);
        }

        private struct EntryStorageBlock : IEquatable<EntryStorageBlock>
        {
            private const int DefaultStorageBlockSize = 40000;
            private readonly byte[] _bytes;
            private readonly GCHandle _gcHandle;
            public readonly byte* BasePtr;

            private uint _unusedStart;
            public uint RemainingSize => (uint)_bytes.Length - _unusedStart;

            public uint TotalSize => (uint)_bytes.Length;

            public bool Alloc(uint size, out void* ptr)
            {
                if (RemainingSize < size)
                {
                    ptr = null;
                    return false;
                }
                else
                {
                    ptr = (BasePtr + _unusedStart);
                    _unusedStart += size;
                    return true;
                }
            }

            private EntryStorageBlock(int storageBlockSize)
            {
                _bytes = new byte[storageBlockSize];
                _gcHandle = GCHandle.Alloc(_bytes, GCHandleType.Pinned);
                BasePtr = (byte*)_gcHandle.AddrOfPinnedObject().ToPointer();
                _unusedStart = 0;
            }

            public static EntryStorageBlock New()
            {
                return new EntryStorageBlock(DefaultStorageBlockSize);
            }

            public void Free()
            {
                _gcHandle.Free();
            }

            internal void Clear()
            {
                _unusedStart = 0;
                Util.ClearArray(_bytes);
            }

            public bool Equals(EntryStorageBlock other)
            {
                return _bytes == other._bytes;
            }
        }
    }

    /// <summary>
    /// A handle for an object stored in some List.
    /// </summary>
    /// <typeparam name="T">The type of object to track.</typeparam>
    internal struct Tracked<T> where T : class
    {
        private readonly int _index;

        public T Get(List<object> list) => (T)list[_index];

        public Tracked(List<object> list, T item)
        {
            _index = list.Count;
            list.Add(item);
        }
    }
}
