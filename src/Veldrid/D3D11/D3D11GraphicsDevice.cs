using Vortice;
using Vortice.Direct3D11;
using Vortice.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Vortice.Mathematics;
using Vortice.Direct3D11.Debug;
using VorticeDXGI = Vortice.DXGI.DXGI;
using VorticeD3D11 = Vortice.Direct3D11.D3D11;

namespace Veldrid.D3D11
{
    internal class D3D11GraphicsDevice : GraphicsDevice
    {
        private readonly IDXGIAdapter _dxgiAdapter;
        private readonly ID3D11Device _device;
        private readonly string _deviceName;
        private readonly ID3D11DeviceContext _immediateContext;
        private readonly D3D11ResourceFactory _d3d11ResourceFactory;
        private readonly D3D11Swapchain _mainSwapchain;
        private readonly bool _supportsConcurrentResources;
        private readonly bool _supportsCommandLists;
        private readonly object _immediateContextLock = new object();
        private readonly BackendInfoD3D11 _d3d11Info;

        private readonly object _mappedResourceLock = new object();
        private readonly Dictionary<MappedResourceCacheKey, MappedResourceInfo> _mappedResources
            = new Dictionary<MappedResourceCacheKey, MappedResourceInfo>();

        private readonly object _stagingResourcesLock = new object();
        private readonly List<D3D11Buffer> _availableStagingBuffers = new List<D3D11Buffer>();

        public override string DeviceName => _deviceName;

        public override GraphicsBackend BackendType => GraphicsBackend.Direct3D11;

        public override bool IsUvOriginTopLeft => true;

        public override bool IsDepthRangeZeroToOne => true;

        public override bool IsClipSpaceYInverted => false;

        public override ResourceFactory ResourceFactory => _d3d11ResourceFactory;

        public ID3D11Device Device => _device;

        public IDXGIAdapter Adapter => _dxgiAdapter;

        public bool IsDebugEnabled { get; }

        public bool SupportsConcurrentResources => _supportsConcurrentResources;

        public bool SupportsCommandLists => _supportsCommandLists;

        public override Swapchain MainSwapchain => _mainSwapchain;

        public override GraphicsDeviceFeatures Features { get; }

        public D3D11GraphicsDevice(GraphicsDeviceOptions options, D3D11DeviceOptions d3D11DeviceOptions, SwapchainDescription? swapchainDesc)
            : this(MergeOptions(d3D11DeviceOptions, options), swapchainDesc)
        {
        }

        public D3D11GraphicsDevice(D3D11DeviceOptions options, SwapchainDescription? swapchainDesc)
        {
            var flags = (DeviceCreationFlags)options.DeviceCreationFlags;
#if DEBUG
            flags |= DeviceCreationFlags.Debug;
#endif
            // If debug flag set but SDK layers aren't available we can't enable debug.
            if (0 != (flags & DeviceCreationFlags.Debug) && !Vortice.Direct3D11.D3D11.SdkLayersAvailable())
            {
                flags &= ~DeviceCreationFlags.Debug;
            }

            try
            {
                if (options.AdapterPtr != IntPtr.Zero)
                {
                    VorticeD3D11.D3D11CreateDevice(options.AdapterPtr,
                        Vortice.Direct3D.DriverType.Hardware,
                        flags,
                        new[]
                        {
                            Vortice.Direct3D.FeatureLevel.Level_11_1,
                            Vortice.Direct3D.FeatureLevel.Level_11_0,
                        },
                        out _device).CheckError();
                }
                else
                {
                    VorticeD3D11.D3D11CreateDevice(IntPtr.Zero,
                        Vortice.Direct3D.DriverType.Hardware,
                        flags,
                        new[]
                        {
                            Vortice.Direct3D.FeatureLevel.Level_11_1,
                            Vortice.Direct3D.FeatureLevel.Level_11_0,
                        },
                        out _device).CheckError();
                }
            }
            catch
            {
                VorticeD3D11.D3D11CreateDevice(IntPtr.Zero,
                    Vortice.Direct3D.DriverType.Hardware,
                    flags,
                    null,
                    out _device).CheckError();
            }

            using (IDXGIDevice dxgiDevice = _device.QueryInterface<IDXGIDevice>())
            {
                // Store a pointer to the DXGI adapter.
                // This is for the case of no preferred DXGI adapter, or fallback to WARP.
                dxgiDevice.GetAdapter(out _dxgiAdapter).CheckError();
                _deviceName = _dxgiAdapter.Description.Description;
            }

            if (swapchainDesc != null)
            {
                SwapchainDescription desc = swapchainDesc.Value;
                _mainSwapchain = new D3D11Swapchain(this, ref desc);
            }
            _immediateContext = _device.ImmediateContext;
            _device.CheckThreadingSupport(out _supportsConcurrentResources, out _supportsCommandLists);

            IsDebugEnabled = (flags & DeviceCreationFlags.Debug) != 0;

            Features = new GraphicsDeviceFeatures(
                computeShader: true,
                geometryShader: true,
                tessellationShaders: true,
                multipleViewports: true,
                samplerLodBias: true,
                drawBaseVertex: true,
                drawBaseInstance: true,
                drawIndirect: true,
                drawIndirectBaseInstance: true,
                fillModeWireframe: true,
                samplerAnisotropy: true,
                depthClipDisable: true,
                texture1D: true,
                independentBlend: true,
                structuredBuffer: true,
                subsetTextureView: true,
                commandListDebugMarkers: _device.FeatureLevel >= Vortice.Direct3D.FeatureLevel.Level_11_1,
                bufferRangeBinding: _device.FeatureLevel >= Vortice.Direct3D.FeatureLevel.Level_11_1,
                shaderFloat64: _device.CheckFeatureSupport<FeatureDataDoubles>(Vortice.Direct3D11.Feature.Doubles).DoublePrecisionFloatShaderOps);

            _d3d11ResourceFactory = new D3D11ResourceFactory(this);
            _d3d11Info = new BackendInfoD3D11(this);

            PostDeviceCreated();
        }

        private static D3D11DeviceOptions MergeOptions(D3D11DeviceOptions d3D11DeviceOptions, GraphicsDeviceOptions options)
        {
            if (options.Debug)
            {
                d3D11DeviceOptions.DeviceCreationFlags |= (uint)DeviceCreationFlags.Debug;
            }

            return d3D11DeviceOptions;
        }

        private protected override void SubmitCommandsCore(CommandList cl, Fence fence)
        {
            D3D11CommandList d3d11CL = Util.AssertSubtype<CommandList, D3D11CommandList>(cl);
            lock (_immediateContextLock)
            {
                if (d3d11CL.DeviceCommandList != null) // CommandList may have been reset in the meantime (resized swapchain).
                {
                    _immediateContext.ExecuteCommandList(d3d11CL.DeviceCommandList, false);
                    d3d11CL.OnCompleted();
                }
            }

            if (fence is D3D11Fence d3d11Fence)
            {
                d3d11Fence.Set();
            }
        }

        private protected override void SwapBuffersCore(Swapchain swapchain)
        {
            lock (_immediateContextLock)
            {
                D3D11Swapchain d3d11SC = Util.AssertSubtype<Swapchain, D3D11Swapchain>(swapchain);
                d3d11SC.DxgiSwapChain.Present(d3d11SC.SyncInterval, PresentFlags.None);
            }
        }

        public override TextureSampleCount GetSampleCountLimit(PixelFormat format, bool depthFormat)
        {
            Format dxgiFormat = D3D11Formats.ToDxgiFormat(format, depthFormat);
            if (CheckFormatMultisample(dxgiFormat, 32))
            {
                return TextureSampleCount.Count32;
            }
            else if (CheckFormatMultisample(dxgiFormat, 16))
            {
                return TextureSampleCount.Count16;
            }
            else if (CheckFormatMultisample(dxgiFormat, 8))
            {
                return TextureSampleCount.Count8;
            }
            else if (CheckFormatMultisample(dxgiFormat, 4))
            {
                return TextureSampleCount.Count4;
            }
            else if (CheckFormatMultisample(dxgiFormat, 2))
            {
                return TextureSampleCount.Count2;
            }

            return TextureSampleCount.Count1;
        }

        private bool CheckFormatMultisample(Format format, int sampleCount)
        {
            return _device.CheckMultisampleQualityLevels(format, sampleCount) != 0;
        }

        private protected override bool GetPixelFormatSupportCore(
            PixelFormat format,
            TextureType type,
            TextureUsage usage,
            out PixelFormatProperties properties)
        {
            if (D3D11Formats.IsUnsupportedFormat(format))
            {
                properties = default(PixelFormatProperties);
                return false;
            }

            Format dxgiFormat = D3D11Formats.ToDxgiFormat(format, (usage & TextureUsage.DepthStencil) != 0);
            FormatSupport fs = _device.CheckFormatSupport(dxgiFormat);

            if ((usage & TextureUsage.RenderTarget) != 0 && (fs & FormatSupport.RenderTarget) == 0
                || (usage & TextureUsage.DepthStencil) != 0 && (fs & FormatSupport.DepthStencil) == 0
                || (usage & TextureUsage.Sampled) != 0 && (fs & FormatSupport.ShaderSample) == 0
                || (usage & TextureUsage.Cubemap) != 0 && (fs & FormatSupport.TextureCube) == 0
                || (usage & TextureUsage.Storage) != 0 && (fs & FormatSupport.TypedUnorderedAccessView) == 0)
            {
                properties = default(PixelFormatProperties);
                return false;
            }

            const uint MaxTextureDimension = 16384;
            const uint MaxVolumeExtent = 2048;

            uint sampleCounts = 0;
            if (CheckFormatMultisample(dxgiFormat, 1)) { sampleCounts |= (1 << 0); }
            if (CheckFormatMultisample(dxgiFormat, 2)) { sampleCounts |= (1 << 1); }
            if (CheckFormatMultisample(dxgiFormat, 4)) { sampleCounts |= (1 << 2); }
            if (CheckFormatMultisample(dxgiFormat, 8)) { sampleCounts |= (1 << 3); }
            if (CheckFormatMultisample(dxgiFormat, 16)) { sampleCounts |= (1 << 4); }
            if (CheckFormatMultisample(dxgiFormat, 32)) { sampleCounts |= (1 << 5); }

            properties = new PixelFormatProperties(
                MaxTextureDimension,
                type == TextureType.Texture1D ? 1 : MaxTextureDimension,
                type != TextureType.Texture3D ? 1 : MaxVolumeExtent,
                uint.MaxValue,
                type == TextureType.Texture3D ? 1 : MaxVolumeExtent,
                sampleCounts);
            return true;
        }

        protected override MappedResource MapCore(MappableResource resource, MapMode mode, uint subresource)
        {
            MappedResourceCacheKey key = new MappedResourceCacheKey(resource, subresource);
            lock (_mappedResourceLock)
            {
                if (_mappedResources.TryGetValue(key, out MappedResourceInfo info))
                {
                    if (info.Mode != mode)
                    {
                        throw new VeldridException("The given resource was already mapped with a different MapMode.");
                    }

                    info.RefCount += 1;
                    _mappedResources[key] = info;
                }
                else
                {
                    // No current mapping exists -- create one.

                    if (resource is D3D11Buffer buffer)
                    {
                        lock (_immediateContextLock)
                        {
                            MappedSubresource msr = _immediateContext.Map(
                                buffer.Buffer,
                                0,
                                D3D11Formats.VdToD3D11MapMode((buffer.Usage & BufferUsage.Dynamic) == BufferUsage.Dynamic, mode),
                                Vortice.Direct3D11.MapFlags.None);

                            info.MappedResource = new MappedResource(resource, mode, msr.DataPointer, buffer.SizeInBytes);
                            info.RefCount = 1;
                            info.Mode = mode;
                            _mappedResources.Add(key, info);
                        }
                    }
                    else
                    {
                        D3D11Texture texture = Util.AssertSubtype<MappableResource, D3D11Texture>(resource);
                        lock (_immediateContextLock)
                        {
                            Util.GetMipLevelAndArrayLayer(texture, subresource, out uint mipLevel, out uint arrayLayer);
                            MappedSubresource msr = _immediateContext.Map(
                                texture.DeviceTexture,
                                (int)mipLevel,
                                (int)arrayLayer,
                                D3D11Formats.VdToD3D11MapMode(false, mode),
                                Vortice.Direct3D11.MapFlags.None,
                                out int mipSize);

                            info.MappedResource = new MappedResource(
                                resource,
                                mode,
                                msr.DataPointer,
                                texture.Height * (uint)msr.RowPitch,
                                subresource,
                                (uint)msr.RowPitch,
                                (uint)msr.DepthPitch);
                            info.RefCount = 1;
                            info.Mode = mode;
                            _mappedResources.Add(key, info);
                        }
                    }
                }

                return info.MappedResource;
            }
        }

        protected override void UnmapCore(MappableResource resource, uint subresource)
        {
            MappedResourceCacheKey key = new MappedResourceCacheKey(resource, subresource);
            bool commitUnmap;

            lock (_mappedResourceLock)
            {
                if (!_mappedResources.TryGetValue(key, out MappedResourceInfo info))
                {
                    throw new VeldridException($"The given resource ({resource}) is not mapped.");
                }

                info.RefCount -= 1;
                commitUnmap = info.RefCount == 0;
                if (commitUnmap)
                {
                    lock (_immediateContextLock)
                    {
                        if (resource is D3D11Buffer buffer)
                        {
                            _immediateContext.Unmap(buffer.Buffer, 0);
                        }
                        else
                        {
                            D3D11Texture texture = Util.AssertSubtype<MappableResource, D3D11Texture>(resource);
                            _immediateContext.Unmap(texture.DeviceTexture, (int)subresource);
                        }

                        bool result = _mappedResources.Remove(key);
                        Debug.Assert(result);
                    }
                }
                else
                {
                    _mappedResources[key] = info;
                }
            }
        }

        private protected unsafe override void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            D3D11Buffer d3dBuffer = Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(buffer);
            if (sizeInBytes == 0)
            {
                return;
            }

            bool isDynamic = (buffer.Usage & BufferUsage.Dynamic) == BufferUsage.Dynamic;
            bool isStaging = (buffer.Usage & BufferUsage.Staging) == BufferUsage.Staging;
            bool isUniformBuffer = (buffer.Usage & BufferUsage.UniformBuffer) == BufferUsage.UniformBuffer;
            bool updateFullBuffer = bufferOffsetInBytes == 0 && sizeInBytes == buffer.SizeInBytes;
            bool useUpdateSubresource = (!isDynamic && !isStaging) && (!isUniformBuffer || updateFullBuffer);
            bool useMap = (isDynamic && updateFullBuffer) || isStaging;

            if (useUpdateSubresource)
            {
                Box? subregion = new Box((int)bufferOffsetInBytes, 0, 0, (int)(sizeInBytes + bufferOffsetInBytes), 1, 1);

                if (isUniformBuffer)
                {
                    subregion = null;
                }

                lock (_immediateContextLock)
                {
                    _immediateContext.UpdateSubresource(d3dBuffer.Buffer, 0, subregion, source, 0, 0);
                }
            }
            else if (useMap)
            {
                MappedResource mr = MapCore(buffer, MapMode.Write, 0);
                if (sizeInBytes < 1024)
                {
                    Unsafe.CopyBlock((byte*)mr.Data + bufferOffsetInBytes, source.ToPointer(), sizeInBytes);
                }
                else
                {
                    Buffer.MemoryCopy(
                        source.ToPointer(),
                        (byte*)mr.Data + bufferOffsetInBytes,
                        buffer.SizeInBytes,
                        sizeInBytes);
                }
                UnmapCore(buffer, 0);
            }
            else
            {
                D3D11Buffer staging = GetFreeStagingBuffer(sizeInBytes);
                UpdateBuffer(staging, 0, source, sizeInBytes);
                Box sourceRegion = new Box(0, 0, 0, (int)sizeInBytes, 1, 1);
                lock (_immediateContextLock)
                {
                    _immediateContext.CopySubresourceRegion(
                        d3dBuffer.Buffer, 0, (int)bufferOffsetInBytes, 0, 0,
                        staging.Buffer, 0,
                        sourceRegion);
                }

                lock (_stagingResourcesLock)
                {
                    _availableStagingBuffers.Add(staging);
                }
            }
        }

        private D3D11Buffer GetFreeStagingBuffer(uint sizeInBytes)
        {
            lock (_stagingResourcesLock)
            {
                foreach (D3D11Buffer buffer in _availableStagingBuffers)
                {
                    if (buffer.SizeInBytes >= sizeInBytes)
                    {
                        _availableStagingBuffers.Remove(buffer);
                        return buffer;
                    }
                }
            }

            DeviceBuffer staging = ResourceFactory.CreateBuffer(
                new BufferDescription(sizeInBytes, BufferUsage.Staging));

            return Util.AssertSubtype<DeviceBuffer, D3D11Buffer>(staging);
        }

        private protected unsafe override void UpdateTextureCore(
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
            D3D11Texture d3dTex = Util.AssertSubtype<Texture, D3D11Texture>(texture);
            bool useMap = (texture.Usage & TextureUsage.Staging) == TextureUsage.Staging;
            if (useMap)
            {
                uint subresource = texture.CalculateSubresource(mipLevel, arrayLayer);
                MappedResourceCacheKey key = new MappedResourceCacheKey(texture, subresource);
                MappedResource map = MapCore(texture, MapMode.Write, subresource);

                uint denseRowSize = FormatHelpers.GetRowPitch(width, texture.Format);
                uint denseSliceSize = FormatHelpers.GetDepthPitch(denseRowSize, height, texture.Format);

                Util.CopyTextureRegion(
                    source.ToPointer(),
                    0, 0, 0,
                    denseRowSize, denseSliceSize,
                    map.Data.ToPointer(),
                    x, y, z,
                    map.RowPitch, map.DepthPitch,
                    width, height, depth,
                    texture.Format);

                UnmapCore(texture, subresource);
            }
            else
            {
                int subresource = D3D11Util.ComputeSubresource(mipLevel, texture.MipLevels, arrayLayer);
                Box resourceRegion = new Box(
                    left: (int)x,
                    right: (int)(x + width),
                    top: (int)y,
                    front: (int)z,
                    bottom: (int)(y + height),
                    back: (int)(z + depth));

                uint srcRowPitch = FormatHelpers.GetRowPitch(width, texture.Format);
                uint srcDepthPitch = FormatHelpers.GetDepthPitch(srcRowPitch, height, texture.Format);
                lock (_immediateContextLock)
                {
                    _immediateContext.UpdateSubresource(
                        d3dTex.DeviceTexture,
                        subresource,
                        resourceRegion,
                        source,
                        (int)srcRowPitch,
                        (int)srcDepthPitch);
                }
            }
        }

        public override bool WaitForFence(Fence fence, ulong nanosecondTimeout)
        {
            return Util.AssertSubtype<Fence, D3D11Fence>(fence).Wait(nanosecondTimeout);
        }

        public override bool WaitForFences(Fence[] fences, bool waitAll, ulong nanosecondTimeout)
        {
            int msTimeout;
            if (nanosecondTimeout == ulong.MaxValue)
            {
                msTimeout = -1;
            }
            else
            {
                msTimeout = (int)Math.Min(nanosecondTimeout / 1_000_000, int.MaxValue);
            }

            ManualResetEvent[] events = GetResetEventArray(fences.Length);
            for (int i = 0; i < fences.Length; i++)
            {
                events[i] = Util.AssertSubtype<Fence, D3D11Fence>(fences[i]).ResetEvent;
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
            Util.AssertSubtype<Fence, D3D11Fence>(fence).Reset();
        }

        internal override uint GetUniformBufferMinOffsetAlignmentCore() => 256u;

        internal override uint GetStructuredBufferMinOffsetAlignmentCore() => 16;

        protected override void PlatformDispose()
        {
            // Dispose staging buffers
            foreach (DeviceBuffer buffer in _availableStagingBuffers)
            {
                buffer.Dispose();
            }
            _availableStagingBuffers.Clear();

            _d3d11ResourceFactory.Dispose();
            _mainSwapchain?.Dispose();
            _immediateContext.Dispose();

            if (IsDebugEnabled)
            {
                uint refCount = _device.Release();
                if (refCount > 0)
                {
                    ID3D11Debug deviceDebug = _device.QueryInterfaceOrNull<ID3D11Debug>();
                    if (deviceDebug != null)
                    {
                        deviceDebug.ReportLiveDeviceObjects(ReportLiveDeviceObjectFlags.Summary | ReportLiveDeviceObjectFlags.Detail | ReportLiveDeviceObjectFlags.IgnoreInternal);
                        deviceDebug.Dispose();
                    }
                }

                _dxgiAdapter.Dispose();

                // Report live objects using DXGI if available (DXGIGetDebugInterface1 will fail on pre Windows 8 OS).
                if (VorticeDXGI.DXGIGetDebugInterface1(out IDXGIDebug1 dxgiDebug).Success)
                {
                    dxgiDebug.ReportLiveObjects(VorticeDXGI.All, ReportLiveObjectFlags.Summary | ReportLiveObjectFlags.IgnoreInternal);
                    dxgiDebug.Dispose();
                }
            }
            else
            {
                _device.Dispose();
                _dxgiAdapter.Dispose();
            }
        }

        private protected override void WaitForIdleCore()
        {
        }

        public override bool GetD3D11Info(out BackendInfoD3D11 info)
        {
            info = _d3d11Info;
            return true;
        }
    }
}
