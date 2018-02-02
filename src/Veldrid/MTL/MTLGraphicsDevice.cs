using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal unsafe class MTLGraphicsDevice : GraphicsDevice
    {
        private readonly MTLDevice _device;
        private CAMetalLayer _metalLayer;
        private readonly MTLCommandQueue _commandQueue;
        private readonly TextureSampleCount _maxSampleCount;
        private readonly MTLSwapchainFramebuffer _swapchainFB;

        private readonly object _submittedCommandsLock = new object();
        private readonly List<(MTLCommandBuffer, MTLFence)> _submittedCBs = new List<(MTLCommandBuffer, MTLFence)>();

        public MTLDevice Device => _device;
        public MTLCommandQueue CommandQueue => _commandQueue;

        public MTLGraphicsDevice(
            GraphicsDeviceOptions options,
            IntPtr nsWindow)
        {
            _device = MTLDevice.MTLCreateSystemDefaultDevice();

            NSWindow nswindow = new NSWindow(nsWindow);
            CGSize windowContentSize = nswindow.contentView.frame.size;
            uint width = (uint)windowContentSize.width;
            uint height = (uint)windowContentSize.height;

            var contentView = nswindow.contentView;
            contentView.wantsLayer = true;

            _metalLayer = CAMetalLayer.New();
            contentView.layer = _metalLayer.NativePtr;
            _metalLayer.device = _device;
            _metalLayer.pixelFormat = MTLPixelFormat.BGRA8Unorm;
            _metalLayer.framebufferOnly = true;

            ResourceFactory = new MTLResourceFactory(this);
            _swapchainFB = new MTLSwapchainFramebuffer(
                this,
                _metalLayer,
                width,
                height,
                options.SwapchainDepthFormat,
                PixelFormat.B8_G8_R8_A8_UNorm);

            _commandQueue = _device.newCommandQueue();
            _swapchainFB.GetNextDrawable();

            foreach (var count in (TextureSampleCount[])Enum.GetValues(typeof(TextureSampleCount)))
            {
                uint uintValue = FormatHelpers.GetSampleCountUInt32(count);
                if (_device.supportsTextureSampleCount((UIntPtr)uintValue))
                {
                    _maxSampleCount = count;
                }
            }

            PostDeviceCreated();
        }

        public override GraphicsBackend BackendType => GraphicsBackend.Metal;

        public override ResourceFactory ResourceFactory { get; }

        public override bool SyncToVerticalBlank { get; set; }

        public override Framebuffer SwapchainFramebuffer => _swapchainFB;

        protected override void SubmitCommandsCore(CommandList commandList, Fence fence)
        {

            MTLCommandList mtlCL = Util.AssertSubtype<CommandList, MTLCommandList>(commandList);
            MTLCommandBuffer cb = mtlCL.Commit();
            lock (_submittedCommandsLock)
            {
                CheckSubmittedCommands(assumeCompletion: false);

                MTLFence mtlFence = fence as MTLFence;
                _submittedCBs.Add((cb, mtlFence));
                ObjectiveCRuntime.retain(cb.NativePtr);
            }
        }

        private void CheckSubmittedCommands(bool assumeCompletion)
        {
            for (int i = 0; i < _submittedCBs.Count; i++)
            {
                (MTLCommandBuffer, MTLFence) pair = _submittedCBs[i];
                if (pair.Item1.status == MTLCommandBufferStatus.Completed)
                {
                    if (pair.Item2 != null)
                    {
                        pair.Item2.Set();
                    }

                    ObjectiveCRuntime.release(pair.Item1.NativePtr);
                    _submittedCBs.RemoveAt(i);
                    i -= 1;
                }
            }
        }

        public override TextureSampleCount GetSampleCountLimit(PixelFormat format, bool depthFormat)
        {
            return _maxSampleCount;
        }

        public override void ResizeMainWindow(uint width, uint height)
        {
            _swapchainFB.Resize(width, height);
            _metalLayer.drawableSize = new CGSize(width, height);
            _swapchainFB.GetNextDrawable();
        }

        protected override void SwapBuffersCore()
        {
            IntPtr currentDrawablePtr = _swapchainFB.CurrentDrawable.NativePtr;
            if (currentDrawablePtr != IntPtr.Zero)
            {
                var submitCB = _commandQueue.commandBuffer();
                submitCB.presentDrawable(currentDrawablePtr);
                submitCB.commit();
                ObjectiveCRuntime.release(submitCB.NativePtr);
            }

            _swapchainFB.GetNextDrawable();
        }

        protected override void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            var mtlBuffer = Util.AssertSubtype<DeviceBuffer, MTLBuffer>(buffer);
            void* destPtr = mtlBuffer.DeviceBuffer.contents();
            byte* destOffsetPtr = (byte*)destPtr + bufferOffsetInBytes;
            Unsafe.CopyBlock(destOffsetPtr, source.ToPointer(), sizeInBytes);
        }

        protected override void UpdateTextureCore(
            Texture texture,
            IntPtr source,
            uint sizeInBytes,
            uint x,
            uint y,
            uint z,
            uint width,
            uint height,
            uint depth,
            uint mipLevel,
            uint arrayLayer)
        {
            MTLTexture mtlTex = Util.AssertSubtype<Texture, MTLTexture>(texture);
            if (mtlTex.StagingBuffer.IsNull)
            {
                Texture stagingTex = ResourceFactory.CreateTexture(new TextureDescription(
                    width, height, depth, 1, 1, texture.Format, TextureUsage.Staging, texture.Type));
                UpdateTexture(stagingTex, source, sizeInBytes, 0, 0, 0, width, height, depth, 0, 0);
                CommandList cl = ResourceFactory.CreateCommandList();
                cl.Begin();
                cl.CopyTexture(
                    stagingTex, 0, 0, 0, 0, 0,
                    texture, x, y, z, mipLevel, arrayLayer,
                    width, height, depth, 1);
                cl.End();
                SubmitCommands(cl);

                cl.Dispose();
                stagingTex.Dispose();
            }
            else
            {
                uint pixelSize = FormatHelpers.GetSizeInBytes(mtlTex.Format);
                uint blockSize = FormatHelpers.IsCompressedFormat(mtlTex.Format) ? 4u : 1u;
                mtlTex.GetSubresourceLayout(mipLevel, arrayLayer, out uint dstRowPitch, out uint dstDepthPitch);
                ulong dstOffset = Util.ComputeSubresourceOffset(mtlTex, mipLevel, arrayLayer);
                if (blockSize == 1)
                {
                    uint srcRowPitch = width * pixelSize;
                    uint srcDepthPitch = srcRowPitch * height;
                    for (uint zz = 0; zz < depth; zz++)
                        for (uint yy = 0; yy < height; yy++)
                        {
                            byte* srcRowBase = (byte*)source.ToPointer()
                                + srcDepthPitch * zz
                                + srcRowPitch * yy;
                            byte* dstRowBase = (byte*)mtlTex.StagingBuffer.contents()
                                + dstOffset
                                + dstDepthPitch * (zz + z)
                                + dstRowPitch * (yy + y)
                                + pixelSize * x;
                            Unsafe.CopyBlock(dstRowBase, srcRowBase, width * pixelSize);
                        }
                }
                else
                {
                    uint numRows = Math.Max(1, height / blockSize);
                    uint paddedWidth = Math.Max(blockSize, width);
                    uint paddedHeight = Math.Max(blockSize, height);

                    uint denseRowSize = paddedWidth * blockSize * pixelSize;
                    uint denseSliceSize = paddedWidth * paddedHeight * pixelSize;
                    for (uint zz = 0; zz < depth; zz++)
                        for (uint row = 0; row < numRows; row++)
                        {
                            byte* srcRowBase = (byte*)source.ToPointer()
                                + denseSliceSize * zz
                                + denseRowSize * row;
                            byte* dstRowBase = (byte*)mtlTex.StagingBuffer.contents()
                                + dstOffset
                                + dstDepthPitch * (zz + z)
                                + dstRowPitch * (row + y)
                                + pixelSize * x;
                            Unsafe.CopyBlock(dstRowBase, srcRowBase, denseRowSize);
                        }
                }
            }
        }

        protected override void WaitForIdleCore()
        {
            lock (_submittedCommandsLock)
            {
                int lastIndex = _submittedCBs.Count - 1;
                if (lastIndex >= 0)
                {
                    (MTLCommandBuffer, MTLFence) lastPair = _submittedCBs[lastIndex];
                    lastPair.Item1.waitUntilCompleted();
                    CheckSubmittedCommands(assumeCompletion: true);
                }
            }
        }

        protected override MappedResource MapCore(MappableResource resource, MapMode mode, uint subresource)
        {
            if (resource is MTLBuffer buffer)
            {
                return MapBuffer(buffer, mode);
            }
            else
            {
                MTLTexture texture = Util.AssertSubtype<MappableResource, MTLTexture>(resource);
                return MapTexture(texture, mode, subresource);
            }
        }

        private MappedResource MapBuffer(MTLBuffer buffer, MapMode mode)
        {
            void* data = buffer.DeviceBuffer.contents();
            return new MappedResource(
                buffer,
                mode,
                (IntPtr)data,
                buffer.SizeInBytes,
                0,
                buffer.SizeInBytes,
                buffer.SizeInBytes);
        }

        private MappedResource MapTexture(MTLTexture texture, MapMode mode, uint subresource)
        {
            Debug.Assert(!texture.StagingBuffer.IsNull);
            void* data = texture.StagingBuffer.contents();
            Util.GetMipLevelAndArrayLayer(texture, subresource, out uint mipLevel, out uint arrayLayer);
            Util.GetMipDimensions(texture, mipLevel, out uint width, out uint height, out uint depth);
            uint pixelSize = FormatHelpers.GetSizeInBytes(texture.Format);
            uint subresourceSize = texture.GetSubresourceSize(mipLevel, arrayLayer);
            uint rowPitch = width * pixelSize;
            uint depthPitch = rowPitch * height;
            ulong offset = Util.ComputeSubresourceOffset(texture, mipLevel, arrayLayer);
            byte* offsetPtr = (byte*)data + offset;
            return new MappedResource(texture, mode, (IntPtr)offsetPtr, subresourceSize, subresource, rowPitch, depthPitch);
        }

        protected override void PlatformDispose()
        {
            WaitForIdle();
            _swapchainFB.Dispose();
            ObjectiveCRuntime.release(_commandQueue.NativePtr);
            ObjectiveCRuntime.release(_metalLayer.NativePtr);
            ObjectiveCRuntime.release(_device.NativePtr);
        }

        protected override void UnmapCore(MappableResource resource, uint subresource)
        {
        }

        public override bool WaitForFence(Fence fence, ulong nanosecondTimeout)
        {
            return Util.AssertSubtype<Fence, MTLFence>(fence).Wait(nanosecondTimeout);
        }

        public override bool WaitForFences(Fence[] fences, bool waitAll, ulong nanosecondTimeout)
        {
            int msTimeout = (int)(nanosecondTimeout / 1_000_000);
            ManualResetEvent[] events = GetResetEventArray(fences.Length);
            for (int i = 0; i < fences.Length; i++)
            {
                events[i] = Util.AssertSubtype<Fence, MTLFence>(fences[i]).ResetEvent;
            }
            bool result;
            if (waitAll)
            {
                result = WaitHandle.WaitAll(events, msTimeout);
            }
            else
            {
                int index = WaitHandle.WaitAny(events, msTimeout);
                result = index != WaitHandle.WaitTimeout;
            }

            ReturnResetEventArray(events);

            return result;
        }

        private readonly object _resetEventsLock = new object();
        private readonly List<ManualResetEvent[]> _resetEvents = new List<ManualResetEvent[]>();

        private ManualResetEvent[] GetResetEventArray(int length)
        {
            lock (_resetEventsLock)
            {
                for (int i = _resetEvents.Count - 1; i > 0; i--)
                {
                    ManualResetEvent[] array = _resetEvents[i];
                    if (array.Length == length)
                    {
                        _resetEvents.RemoveAt(i);
                        return array;
                    }
                }
            }

            ManualResetEvent[] newArray = new ManualResetEvent[length];
            return newArray;
        }

        private void ReturnResetEventArray(ManualResetEvent[] array)
        {
            lock (_resetEventsLock)
            {
                _resetEvents.Add(array);
            }
        }

        public override void ResetFence(Fence fence)
        {
            Util.AssertSubtype<Fence, MTLFence>(fence).Reset();
        }
    }
}
