using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid
{
    /// <summary>
    /// Represents an abstract graphics device, capable of creating device resources and executing commands.
    /// </summary>
    public abstract class GraphicsDevice : IDisposable
    {
        private readonly object _deferredDisposalLock = new object();
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly List<IDisposable> _autoDisposables = new List<IDisposable>();
        private long _autoDisposeThreshold;
        private long _autoDisposeCounter;
        private Sampler _aniso4xSampler;

        internal GraphicsDevice() { }

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        public abstract string DeviceName { get; }

        /// <summary>
        /// Gets a value identifying the specific graphics API used by this instance.
        /// </summary>
        public abstract GraphicsBackend BackendType { get; }

        /// <summary>
        /// Gets a value identifying whether texture coordinates begin in the top left corner of a Texture.
        /// If true, (0, 0) refers to the top-left texel of a Texture. If false, (0, 0) refers to the bottom-left
        /// texel of a Texture. This property is useful for determining how the output of a Framebuffer should be sampled.
        /// </summary>
        public abstract bool IsUvOriginTopLeft { get; }

        /// <summary>
        /// Gets a value indicating whether this device's depth values range from 0 to 1.
        /// If false, depth values instead range from -1 to 1.
        /// </summary>
        public abstract bool IsDepthRangeZeroToOne { get; }

        /// <summary>
        /// Gets a value indicating whether this device's clip space Y values increase from top (-1) to bottom (1).
        /// If false, clip space Y values instead increase from bottom (-1) to top (1).
        /// </summary>
        public abstract bool IsClipSpaceYInverted { get; }

        /// <summary>
        /// Gets the <see cref="ResourceFactory"/> controlled by this instance.
        /// </summary>
        public abstract ResourceFactory ResourceFactory { get; }

        /// <summary>
        /// Retrieves the main Swapchain for this device. This property is only valid if the device was created with a main
        /// Swapchain, and will return null otherwise.
        /// </summary>
        public abstract Swapchain MainSwapchain { get; }

        /// <summary>
        /// Gets a <see cref="GraphicsDeviceFeatures"/> which enumerates the optional features supported by this instance.
        /// </summary>
        public abstract GraphicsDeviceFeatures Features { get; }

        /// <summary>
        /// Gets or sets whether the main Swapchain's <see cref="SwapBuffers()"/> should be synchronized to the window system's
        /// vertical refresh rate.
        /// This is equivalent to <see cref="MainSwapchain"/>.<see cref="Swapchain.SyncToVerticalBlank"/>.
        /// This property cannot be set if this GraphicsDevice was created without a main Swapchain.
        /// </summary>
        public virtual bool SyncToVerticalBlank
        {
            get => MainSwapchain?.SyncToVerticalBlank ?? false;
            set
            {
                if (MainSwapchain == null)
                {
                    throw new VeldridException($"This GraphicsDevice was created without a main Swapchain. This property cannot be set.");
                }

                MainSwapchain.SyncToVerticalBlank = value;
            }
        }

        /// <summary>
        /// The required alignment, in bytes, for uniform buffer offsets. <see cref="DeviceBufferRange.Offset"/> must be a
        /// multiple of this value. When binding a <see cref="ResourceSet"/> to a <see cref="CommandList"/> with an overload
        /// accepting dynamic offsets, each offset must be a multiple of this value.
        /// </summary>
        public uint UniformBufferMinOffsetAlignment => GetUniformBufferMinOffsetAlignmentCore();

        /// <summary>
        /// The required alignment, in bytes, for structured buffer offsets. <see cref="DeviceBufferRange.Offset"/> must be a
        /// multiple of this value. When binding a <see cref="ResourceSet"/> to a <see cref="CommandList"/> with an overload
        /// accepting dynamic offsets, each offset must be a multiple of this value.
        /// </summary>
        public uint StructuredBufferMinOffsetAlignment => GetStructuredBufferMinOffsetAlignmentCore();

        /// <summary>
        /// A threshold to control the automatic disposals of <see cref="IDisposable"/> objects added through <see cref="AutoDisposeWhenIdle(IDisposable, long)"/>.
        /// <br/>
        /// When the total estimated size from <see cref="AddAutoDisposePressure(long)"/> or <see cref="AutoDisposeWhenIdle(IDisposable, long)"/>
        /// reaches the given threshold value, this graphics device will block until it becomes idle,
        /// and then it will dispose all objects pending for dispose added through <see cref="AutoDisposeWhenIdle(IDisposable, long)"/>.
        /// <br/>
        /// In particular, a threshold of 0 will cause <see cref="AutoDisposeWhenIdle(IDisposable, long)"/> with a non-negative hint size to block on each call.
        /// <br/>
        /// To maintain backward compatibility, <see cref="IDisposable"/> objects added through <see cref="DisposeWhenIdle(IDisposable)"/>
        /// will <i>not</i> be disposed automatically, they still need to be disposed by using <see cref="WaitForIdle()"/> or <see cref="Dispose()"/>.
        /// </summary>
        public long AutoDisposeThreshold
        {
            get { lock (_deferredDisposalLock) { return _autoDisposeThreshold; } }
            set
            {
                lock (_deferredDisposalLock) { _autoDisposeThreshold = value; }
                AddAutoDisposePressure(0); // starts a check
            }
        }

        internal abstract uint GetUniformBufferMinOffsetAlignmentCore();
        internal abstract uint GetStructuredBufferMinOffsetAlignmentCore();

        /// <summary>
        /// Submits the given <see cref="CommandList"/> for execution by this device.
        /// Commands submitted in this way may not be completed when this method returns.
        /// Use <see cref="WaitForIdle"/> to wait for all submitted commands to complete.
        /// <see cref="CommandList.End"/> must have been called on <paramref name="commandList"/> for this method to succeed.
        /// </summary>
        /// <param name="commandList">The completed <see cref="CommandList"/> to execute. <see cref="CommandList.End"/> must have
        /// been previously called on this object.</param>
        public void SubmitCommands(CommandList commandList) => SubmitCommandsCore(commandList, null);

        /// <summary>
        /// Submits the given <see cref="CommandList"/> for execution by this device.
        /// Commands submitted in this way may not be completed when this method returns.
        /// Use <see cref="WaitForIdle"/> to wait for all submitted commands to complete.
        /// <see cref="CommandList.End"/> must have been called on <paramref name="commandList"/> for this method to succeed.
        /// </summary>
        /// <param name="commandList">The completed <see cref="CommandList"/> to execute. <see cref="CommandList.End"/> must have
        /// been previously called on this object.</param>
        /// <param name="fence">A <see cref="Fence"/> which will become signaled after this submission fully completes
        /// execution.</param>
        public void SubmitCommands(CommandList commandList, Fence fence) => SubmitCommandsCore(commandList, fence);

        private protected abstract void SubmitCommandsCore(
            CommandList commandList,
            Fence fence);

        /// <summary>
        /// Blocks the calling thread until the given <see cref="Fence"/> becomes signaled.
        /// </summary>
        /// <param name="fence">The <see cref="Fence"/> instance to wait on.</param>
        public void WaitForFence(Fence fence)
        {
            if (!WaitForFence(fence, ulong.MaxValue))
            {
                throw new VeldridException("The operation timed out before the Fence was signaled.");
            }
        }

        /// <summary>
        /// Blocks the calling thread until the given <see cref="Fence"/> becomes signaled, or until a time greater than the
        /// given TimeSpan has elapsed.
        /// </summary>
        /// <param name="fence">The <see cref="Fence"/> instance to wait on.</param>
        /// <param name="timeout">A TimeSpan indicating the maximum time to wait on the Fence.</param>
        /// <returns>True if the Fence was signaled. False if the timeout was reached instead.</returns>
        public bool WaitForFence(Fence fence, TimeSpan timeout)
            => WaitForFence(fence, (ulong)timeout.TotalMilliseconds * 1_000_000);
        /// <summary>
        /// Blocks the calling thread until the given <see cref="Fence"/> becomes signaled, or until a time greater than the
        /// given TimeSpan has elapsed.
        /// </summary>
        /// <param name="fence">The <see cref="Fence"/> instance to wait on.</param>
        /// <param name="nanosecondTimeout">A value in nanoseconds, indicating the maximum time to wait on the Fence.</param>
        /// <returns>True if the Fence was signaled. False if the timeout was reached instead.</returns>
        public abstract bool WaitForFence(Fence fence, ulong nanosecondTimeout);

        /// <summary>
        /// Blocks the calling thread until one or all of the given <see cref="Fence"/> instances have become signaled.
        /// </summary>
        /// <param name="fences">An array of <see cref="Fence"/> objects to wait on.</param>
        /// <param name="waitAll">If true, then this method blocks until all of the given Fences become signaled.
        /// If false, then this method only waits until one of the Fences become signaled.</param>
        public void WaitForFences(Fence[] fences, bool waitAll)
        {
            if (!WaitForFences(fences, waitAll, ulong.MaxValue))
            {
                throw new VeldridException("The operation timed out before the Fence(s) were signaled.");
            }
        }

        /// <summary>
        /// Blocks the calling thread until one or all of the given <see cref="Fence"/> instances have become signaled,
        /// or until the given timeout has been reached.
        /// </summary>
        /// <param name="fences">An array of <see cref="Fence"/> objects to wait on.</param>
        /// <param name="waitAll">If true, then this method blocks until all of the given Fences become signaled.
        /// If false, then this method only waits until one of the Fences become signaled.</param>
        /// <param name="timeout">A TimeSpan indicating the maximum time to wait on the Fences.</param>
        /// <returns>True if the Fence was signaled. False if the timeout was reached instead.</returns>
        public bool WaitForFences(Fence[] fences, bool waitAll, TimeSpan timeout)
            => WaitForFences(fences, waitAll, (ulong)timeout.TotalMilliseconds * 1_000_000);

        /// <summary>
        /// Blocks the calling thread until one or all of the given <see cref="Fence"/> instances have become signaled,
        /// or until the given timeout has been reached.
        /// </summary>
        /// <param name="fences">An array of <see cref="Fence"/> objects to wait on.</param>
        /// <param name="waitAll">If true, then this method blocks until all of the given Fences become signaled.
        /// If false, then this method only waits until one of the Fences become signaled.</param>
        /// <param name="nanosecondTimeout">A value in nanoseconds, indicating the maximum time to wait on the Fence.  Pass ulong.MaxValue to wait indefinitely.</param>
        /// <returns>True if the Fence was signaled. False if the timeout was reached instead.</returns>
        public abstract bool WaitForFences(Fence[] fences, bool waitAll, ulong nanosecondTimeout);

        /// <summary>
        /// Resets the given <see cref="Fence"/> to the unsignaled state.
        /// </summary>
        /// <param name="fence">The <see cref="Fence"/> instance to reset.</param>
        public abstract void ResetFence(Fence fence);

        /// <summary>
        /// Swaps the buffers of the main swapchain and presents the rendered image to the screen.
        /// This is equivalent to passing <see cref="MainSwapchain"/> to <see cref="SwapBuffers(Swapchain)"/>.
        /// This method can only be called if this GraphicsDevice was created with a main Swapchain.
        /// </summary>
        public void SwapBuffers()
        {
            if (MainSwapchain == null)
            {
                throw new VeldridException("This GraphicsDevice was created without a main Swapchain, so the requested operation cannot be performed.");
            }

            SwapBuffers(MainSwapchain);
        }

        /// <summary>
        /// Swaps the buffers of the given swapchain.
        /// </summary>
        /// <param name="swapchain">The <see cref="Swapchain"/> to swap and present.</param>
        public void SwapBuffers(Swapchain swapchain) => SwapBuffersCore(swapchain);

        private protected abstract void SwapBuffersCore(Swapchain swapchain);

        /// <summary>
        /// Gets a <see cref="Framebuffer"/> object representing the render targets of the main swapchain.
        /// This is equivalent to <see cref="MainSwapchain"/>.<see cref="Swapchain.Framebuffer"/>.
        /// If this GraphicsDevice was created without a main Swapchain, then this returns null.
        /// </summary>
        public Framebuffer SwapchainFramebuffer => MainSwapchain?.Framebuffer;

        /// <summary>
        /// Notifies this instance that the main window has been resized. This causes the <see cref="SwapchainFramebuffer"/> to
        /// be appropriately resized and recreated.
        /// This is equivalent to calling <see cref="MainSwapchain"/>.<see cref="Swapchain.Resize(uint, uint)"/>.
        /// This method can only be called if this GraphicsDevice was created with a main Swapchain.
        /// </summary>
        /// <param name="width">The new width of the main window.</param>
        /// <param name="height">The new height of the main window.</param>
        public void ResizeMainWindow(uint width, uint height)
        {
            if (MainSwapchain == null)
            {
                throw new VeldridException("This GraphicsDevice was created without a main Swapchain, so the requested operation cannot be performed.");
            }

            MainSwapchain.Resize(width, height);
        }

        /// <summary>
        /// A blocking method that returns when all submitted <see cref="CommandList"/> objects have fully completed.
        /// </summary>
        public void WaitForIdle()
        {
            WaitForIdleCore();
            FlushDeferredDisposals();
        }

        private protected abstract void WaitForIdleCore();

        /// <summary>
        /// Gets the maximum sample count supported by the given <see cref="PixelFormat"/>.
        /// </summary>
        /// <param name="format">The format to query.</param>
        /// <param name="depthFormat">Whether the format will be used in a depth texture.</param>
        /// <returns>A <see cref="TextureSampleCount"/> value representing the maximum count that a <see cref="Texture"/> of that
        /// format can be created with.</returns>
        public abstract TextureSampleCount GetSampleCountLimit(PixelFormat format, bool depthFormat);

        /// <summary>
        /// Maps a <see cref="DeviceBuffer"/> or <see cref="Texture"/> into a CPU-accessible data region. For Texture resources, this
        /// overload maps the first subresource.
        /// </summary>
        /// <param name="resource">The <see cref="DeviceBuffer"/> or <see cref="Texture"/> resource to map.</param>
        /// <param name="mode">The <see cref="MapMode"/> to use.</param>
        /// <returns>A <see cref="MappedResource"/> structure describing the mapped data region.</returns>
        public MappedResource Map(MappableResource resource, MapMode mode) => Map(resource, mode, 0);
        /// <summary>
        /// Maps a <see cref="DeviceBuffer"/> or <see cref="Texture"/> into a CPU-accessible data region.
        /// </summary>
        /// <param name="resource">The <see cref="DeviceBuffer"/> or <see cref="Texture"/> resource to map.</param>
        /// <param name="mode">The <see cref="MapMode"/> to use.</param>
        /// <param name="subresource">The subresource to map. Subresources are indexed first by mip slice, then by array layer.
        /// For <see cref="DeviceBuffer"/> resources, this parameter must be 0.</param>
        /// <returns>A <see cref="MappedResource"/> structure describing the mapped data region.</returns>
        public MappedResource Map(MappableResource resource, MapMode mode, uint subresource)
        {
#if VALIDATE_USAGE
            if (resource is DeviceBuffer buffer)
            {
                if ((buffer.Usage & BufferUsage.Dynamic) != BufferUsage.Dynamic
                    && (buffer.Usage & BufferUsage.Staging) != BufferUsage.Staging)
                {
                    throw new VeldridException("Buffers must have the Staging or Dynamic usage flag to be mapped.");
                }
                if (subresource != 0)
                {
                    throw new VeldridException("Subresource must be 0 for Buffer resources.");
                }
                if ((mode == MapMode.Read || mode == MapMode.ReadWrite) && (buffer.Usage & BufferUsage.Staging) == 0)
                {
                    throw new VeldridException(
                        $"{nameof(MapMode)}.{nameof(MapMode.Read)} and {nameof(MapMode)}.{nameof(MapMode.ReadWrite)} can only be used on buffers created with {nameof(BufferUsage)}.{nameof(BufferUsage.Staging)}.");
                }
            }
            else if (resource is Texture tex)
            {
                if ((tex.Usage & TextureUsage.Staging) == 0)
                {
                    throw new VeldridException("Texture must have the Staging usage flag to be mapped.");
                }
                if (subresource >= tex.ArrayLayers * tex.MipLevels)
                {
                    throw new VeldridException(
                        "Subresource must be less than the number of subresources in the Texture being mapped.");
                }
            }
#endif

            return MapCore(resource, mode, subresource);
        }

        /// <summary>
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="mode"></param>
        /// <param name="subresource"></param>
        /// <returns></returns>
        protected abstract MappedResource MapCore(MappableResource resource, MapMode mode, uint subresource);

        /// <summary>
        /// Maps a <see cref="DeviceBuffer"/> or <see cref="Texture"/> into a CPU-accessible data region, and returns a structured
        /// view over that region. For Texture resources, this overload maps the first subresource.
        /// </summary>
        /// <param name="resource">The <see cref="DeviceBuffer"/> or <see cref="Texture"/> resource to map.</param>
        /// <param name="mode">The <see cref="MapMode"/> to use.</param>
        /// <typeparam name="T">The blittable value type which mapped data is viewed as.</typeparam>
        /// <returns>A <see cref="MappedResource"/> structure describing the mapped data region.</returns>
        public MappedResourceView<T> Map<T>(MappableResource resource, MapMode mode) where T : unmanaged
            => Map<T>(resource, mode, 0);
        /// <summary>
        /// Maps a <see cref="DeviceBuffer"/> or <see cref="Texture"/> into a CPU-accessible data region, and returns a structured
        /// view over that region.
        /// </summary>
        /// <param name="resource">The <see cref="DeviceBuffer"/> or <see cref="Texture"/> resource to map.</param>
        /// <param name="mode">The <see cref="MapMode"/> to use.</param>
        /// <param name="subresource">The subresource to map. Subresources are indexed first by mip slice, then by array layer.</param>
        /// <typeparam name="T">The blittable value type which mapped data is viewed as.</typeparam>
        /// <returns>A <see cref="MappedResource"/> structure describing the mapped data region.</returns>
        public MappedResourceView<T> Map<T>(MappableResource resource, MapMode mode, uint subresource) where T : unmanaged
        {
            MappedResource mappedResource = Map(resource, mode, subresource);
            return new MappedResourceView<T>(mappedResource);
        }

        /// <summary>
        /// Invalidates a previously-mapped data region for the given <see cref="DeviceBuffer"/> or <see cref="Texture"/>.
        /// For <see cref="Texture"/> resources, this unmaps the first subresource.
        /// </summary>
        /// <param name="resource">The resource to unmap.</param>
        public void Unmap(MappableResource resource) => Unmap(resource, 0);
        /// <summary>
        /// Invalidates a previously-mapped data region for the given <see cref="DeviceBuffer"/> or <see cref="Texture"/>.
        /// </summary>
        /// <param name="resource">The resource to unmap.</param>
        /// <param name="subresource">The subresource to unmap. Subresources are indexed first by mip slice, then by array layer.
        /// For <see cref="DeviceBuffer"/> resources, this parameter must be 0.</param>
        public void Unmap(MappableResource resource, uint subresource)
        {
            UnmapCore(resource, subresource);
        }

        /// <summary>
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="subresource"></param>
        protected abstract void UnmapCore(MappableResource resource, uint subresource);

        /// <summary>
        /// Updates a portion of a <see cref="Texture"/> resource with new data.
        /// </summary>
        /// <param name="texture">The resource to update.</param>
        /// <param name="source">A pointer to the start of the data to upload. This must point to tightly-packed pixel data for
        /// the region specified.</param>
        /// <param name="sizeInBytes">The number of bytes to upload. This value must match the total size of the texture region
        /// specified.</param>
        /// <param name="x">The minimum X value of the updated region.</param>
        /// <param name="y">The minimum Y value of the updated region.</param>
        /// <param name="z">The minimum Z value of the updated region.</param>
        /// <param name="width">The width of the updated region, in texels.</param>
        /// <param name="height">The height of the updated region, in texels.</param>
        /// <param name="depth">The depth of the updated region, in texels.</param>
        /// <param name="mipLevel">The mipmap level to update. Must be less than the total number of mipmaps contained in the
        /// <see cref="Texture"/>.</param>
        /// <param name="arrayLayer">The array layer to update. Must be less than the total array layer count contained in the
        /// <see cref="Texture"/>.</param>
        public void UpdateTexture(
            Texture texture,
            IntPtr source,
            uint sizeInBytes,
            uint x, uint y, uint z,
            uint width, uint height, uint depth,
            uint mipLevel, uint arrayLayer)
        {
#if VALIDATE_USAGE
            ValidateUpdateTextureParameters(texture, sizeInBytes, x, y, z, width, height, depth, mipLevel, arrayLayer);
#endif
            UpdateTextureCore(texture, source, sizeInBytes, x, y, z, width, height, depth, mipLevel, arrayLayer);
        }

        /// <summary>
        /// Updates a portion of a <see cref="Texture"/> resource with new data contained in an array
        /// </summary>
        /// <param name="texture">The resource to update.</param>
        /// <param name="source">An array containing the data to upload. This must contain tightly-packed pixel data for the
        /// region specified.</param>
        /// <param name="x">The minimum X value of the updated region.</param>
        /// <param name="y">The minimum Y value of the updated region.</param>
        /// <param name="z">The minimum Z value of the updated region.</param>
        /// <param name="width">The width of the updated region, in texels.</param>
        /// <param name="height">The height of the updated region, in texels.</param>
        /// <param name="depth">The depth of the updated region, in texels.</param>
        /// <param name="mipLevel">The mipmap level to update. Must be less than the total number of mipmaps contained in the
        /// <see cref="Texture"/>.</param>
        /// <param name="arrayLayer">The array layer to update. Must be less than the total array layer count contained in the
        /// <see cref="Texture"/>.</param>
        public void UpdateTexture<T>(
            Texture texture,
            T[] source,
            uint x, uint y, uint z,
            uint width, uint height, uint depth,
            uint mipLevel, uint arrayLayer) where T : unmanaged
        {
            UpdateTexture(texture, (ReadOnlySpan<T>)source, x, y, z, width, height, depth, mipLevel, arrayLayer);
        }

        /// <summary>
        /// Updates a portion of a <see cref="Texture"/> resource with new data contained in an array
        /// </summary>
        /// <param name="texture">The resource to update.</param>
        /// <param name="source">A readonly span containing the data to upload. This must contain tightly-packed pixel data for the
        /// region specified.</param>
        /// <param name="x">The minimum X value of the updated region.</param>
        /// <param name="y">The minimum Y value of the updated region.</param>
        /// <param name="z">The minimum Z value of the updated region.</param>
        /// <param name="width">The width of the updated region, in texels.</param>
        /// <param name="height">The height of the updated region, in texels.</param>
        /// <param name="depth">The depth of the updated region, in texels.</param>
        /// <param name="mipLevel">The mipmap level to update. Must be less than the total number of mipmaps contained in the
        /// <see cref="Texture"/>.</param>
        /// <param name="arrayLayer">The array layer to update. Must be less than the total array layer count contained in the
        /// <see cref="Texture"/>.</param>
        public unsafe void UpdateTexture<T>(
            Texture texture,
            ReadOnlySpan<T> source,
            uint x, uint y, uint z,
            uint width, uint height, uint depth,
            uint mipLevel, uint arrayLayer) where T : unmanaged
        {
            uint sizeInBytes = (uint)(sizeof(T) * source.Length);
#if VALIDATE_USAGE
            ValidateUpdateTextureParameters(texture, sizeInBytes, x, y, z, width, height, depth, mipLevel, arrayLayer);
#endif

            fixed (void* pin = &MemoryMarshal.GetReference(source))
            {
                UpdateTextureCore(
                texture,
                (IntPtr)pin,
                sizeInBytes,
                x, y, z,
                width, height, depth,
                mipLevel, arrayLayer);
            }
        }

        private protected abstract void UpdateTextureCore(
            Texture texture,
            IntPtr source,
            uint sizeInBytes,
            uint x, uint y, uint z,
            uint width, uint height, uint depth,
            uint mipLevel, uint arrayLayer);

        [Conditional("VALIDATE_USAGE")]
        private static void ValidateUpdateTextureParameters(
            Texture texture,
            uint sizeInBytes,
            uint x, uint y, uint z,
            uint width, uint height, uint depth,
            uint mipLevel, uint arrayLayer)
        {
            if (FormatHelpers.IsCompressedFormat(texture.Format))
            {
                if (x % 4 != 0 || y % 4 != 0 || height % 4 != 0 || width % 4 != 0)
                {
                    Util.GetMipDimensions(texture, mipLevel, out uint mipWidth, out uint mipHeight, out _);
                    if (width != mipWidth && height != mipHeight)
                    {
                        throw new VeldridException($"Updates to block-compressed textures must use a region that is block-size aligned and sized.");
                    }
                }
            }
            uint expectedSize = FormatHelpers.GetRegionSize(width, height, depth, texture.Format);
            if (sizeInBytes < expectedSize)
            {
                throw new VeldridException(
                    $"The data size is less than expected for the given update region. At least {expectedSize} bytes must be provided, but only {sizeInBytes} were.");
            }

            // Compressed textures don't necessarily need to have a Texture.Width and Texture.Height that are a multiple of 4.
            // But the mipdata width and height *does* need to be a multiple of 4.
            uint roundedTextureWidth, roundedTextureHeight;
            if (FormatHelpers.IsCompressedFormat(texture.Format))
            {
                roundedTextureWidth = (texture.Width + 3) / 4 * 4;
                roundedTextureHeight = (texture.Height + 3) / 4 * 4;
            }
            else
            {
                roundedTextureWidth = texture.Width;
                roundedTextureHeight = texture.Height;
            }

            if (x + width > roundedTextureWidth || y + height > roundedTextureHeight || z + depth > texture.Depth)
            {
                throw new VeldridException($"The given region does not fit into the Texture.");
            }

            if (mipLevel >= texture.MipLevels)
            {
                throw new VeldridException(
                    $"{nameof(mipLevel)} ({mipLevel}) must be less than the Texture's mip level count ({texture.MipLevels}).");
            }

            uint effectiveArrayLayers = texture.ArrayLayers;
            if ((texture.Usage & TextureUsage.Cubemap) != 0)
            {
                effectiveArrayLayers *= 6;
            }
            if (arrayLayer >= effectiveArrayLayers)
            {
                throw new VeldridException(
                    $"{nameof(arrayLayer)} ({arrayLayer}) must be less than the Texture's effective array layer count ({effectiveArrayLayers}).");
            }
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/> storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">The value to upload.</param>
        public unsafe void UpdateBuffer<T>(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            T source) where T : unmanaged
        {
            ref byte sourceByteRef = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref source));
            fixed (byte* ptr = &sourceByteRef)
            {
                UpdateBuffer(buffer, bufferOffsetInBytes, (IntPtr)ptr, (uint)sizeof(T));
            }
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A reference to the single value to upload.</param>
        public unsafe void UpdateBuffer<T>(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            ref T source) where T : unmanaged
        {
            ref byte sourceByteRef = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref source));
            fixed (byte* ptr = &sourceByteRef)
            {
                UpdateBuffer(buffer, bufferOffsetInBytes, (IntPtr)ptr, (uint)sizeof(T));
            }
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A reference to the first of a series of values to upload.</param>
        /// <param name="sizeInBytes">The total size of the uploaded data, in bytes.</param>
        public unsafe void UpdateBuffer<T>(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            ref T source,
            uint sizeInBytes) where T : unmanaged
        {
            ref byte sourceByteRef = ref Unsafe.AsRef<byte>(Unsafe.AsPointer(ref source));
            fixed (byte* ptr = &sourceByteRef)
            {
                UpdateBuffer(buffer, bufferOffsetInBytes, (IntPtr)ptr, sizeInBytes);
            }
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">An array containing the data to upload.</param>
        public unsafe void UpdateBuffer<T>(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            T[] source) where T : unmanaged
        {
            UpdateBuffer(buffer, bufferOffsetInBytes, (ReadOnlySpan<T>)source);
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// This function must be used with a blittable value type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of data to upload.</typeparam>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A readonly span containing the data to upload.</param>
        public unsafe void UpdateBuffer<T>(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            ReadOnlySpan<T> source) where T : unmanaged
        {
            fixed (void* pin = &MemoryMarshal.GetReference(source))
            {
                UpdateBuffer(buffer, bufferOffsetInBytes, (IntPtr)pin, (uint)(sizeof(T) * source.Length));
            }
        }

        /// <summary>
        /// Updates a <see cref="DeviceBuffer"/> region with new data.
        /// </summary>
        /// <param name="buffer">The resource to update.</param>
        /// <param name="bufferOffsetInBytes">An offset, in bytes, from the beginning of the <see cref="DeviceBuffer"/>'s storage, at
        /// which new data will be uploaded.</param>
        /// <param name="source">A pointer to the start of the data to upload.</param>
        /// <param name="sizeInBytes">The total size of the uploaded data, in bytes.</param>
        public void UpdateBuffer(
            DeviceBuffer buffer,
            uint bufferOffsetInBytes,
            IntPtr source,
            uint sizeInBytes)
        {
            if (bufferOffsetInBytes + sizeInBytes > buffer.SizeInBytes)
            {
                throw new VeldridException(
                    $"The data size given to UpdateBuffer is too large. The given buffer can only hold {buffer.SizeInBytes} total bytes. The requested update would require {bufferOffsetInBytes + sizeInBytes} bytes.");
            }
            UpdateBufferCore(buffer, bufferOffsetInBytes, source, sizeInBytes);
        }

        private protected abstract void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes);

        /// <summary>
        /// Gets whether or not the given <see cref="PixelFormat"/>, <see cref="TextureType"/>, and <see cref="TextureUsage"/>
        /// combination is supported by this instance.
        /// </summary>
        /// <param name="format">The PixelFormat to query.</param>
        /// <param name="type">The TextureType to query.</param>
        /// <param name="usage">The TextureUsage to query.</param>
        /// <returns>True if the given combination is supported; false otherwise.</returns>
        public bool GetPixelFormatSupport(
            PixelFormat format,
            TextureType type,
            TextureUsage usage)
        {
            return GetPixelFormatSupportCore(format, type, usage, out _);
        }

        /// <summary>
        /// Gets whether or not the given <see cref="PixelFormat"/>, <see cref="TextureType"/>, and <see cref="TextureUsage"/>
        /// combination is supported by this instance, and also gets the device-specific properties supported by this instance.
        /// </summary>
        /// <param name="format">The PixelFormat to query.</param>
        /// <param name="type">The TextureType to query.</param>
        /// <param name="usage">The TextureUsage to query.</param>
        /// <param name="properties">If the combination is supported, then this parameter describes the limits of a Texture
        /// created using the given combination of attributes.</param>
        /// <returns>True if the given combination is supported; false otherwise. If the combination is supported,
        /// then <paramref name="properties"/> contains the limits supported by this instance.</returns>
        public bool GetPixelFormatSupport(
            PixelFormat format,
            TextureType type,
            TextureUsage usage,
            out PixelFormatProperties properties)
        {
            return GetPixelFormatSupportCore(format, type, usage, out properties);
        }

        private protected abstract bool GetPixelFormatSupportCore(
            PixelFormat format,
            TextureType type,
            TextureUsage usage,
            out PixelFormatProperties properties);

        /// <summary>
        /// Adds the given object to a deferred disposal list, which will be processed when this GraphicsDevice becomes idle,
        /// or more specifically, when <see cref="WaitForIdle()"/> has been called.
        /// This method can be used to safely dispose a device resource which may be in use at the time this method is called,
        /// but which will no longer be in use when the device is idle.
        /// </summary>
        /// <param name="disposable">An object to dispose when this instance becomes idle.</param>
        public void DisposeWhenIdle(IDisposable disposable)
        {
            if (disposable == null)
            {
                return;
            }
            lock (_deferredDisposalLock)
            {
                _disposables.Add(disposable);
            }
        }

        /// <summary>
        /// Informs that the given <paramref name="disposable"/> object, with an estimated size of <paramref name="hintSize"/> bytes,
        /// should be disposed when this GraphicsDevice becomes idle.
        /// If these objects pending for dispose, have a total estimated size that exceeds <see cref="AutoDisposeThreshold"/>,
        /// then this method will block until the device becomes idle, and it will subsequently dispose all objects added through this method.
        /// In addition, an explicit call to <see cref="WaitForIdle()"/> or <see cref="Dispose()"/> can also be used to dispose them.
        /// <br/>
        /// This method can be used to safely dispose a device resource which may be in use at the time this method is called,
        /// but which will no longer be in use when the device is idle.
        /// <br/>
        /// To maintain backward compatibility, <see cref="IDisposable"/> objects added through <see cref="DisposeWhenIdle(IDisposable)"/>
        /// will <i>not</i> be automatically disposed by this method.
        /// They still need to be disposed by using <see cref="WaitForIdle()"/> or <see cref="Dispose()"/>.
        /// </summary>
        /// <param name="disposable">An object to dispose when this instance becomes idle.</param>
        /// <param name="hintSize">The estimated size of the given <paramref name="disposable"/> object.</param>
        public void AutoDisposeWhenIdle(IDisposable disposable, long hintSize)
        {
            lock (_deferredDisposalLock)
            {
                if (disposable != null)
                {
                    _autoDisposables.Add(disposable);
                }
                _autoDisposeCounter += hintSize;
                if (_autoDisposeCounter < _autoDisposeThreshold)
                {
                    return;
                }
            }
            // _autoDisposeCounter >= _autoDisposeThreshold
            WaitForIdleCore();
            FlushDeferredAutoDisposals();
        }

        /// <summary>
        /// Updates the estimated size of objects pending for dispose added through <see cref="AutoDisposeWhenIdle(IDisposable, long)"/>.
        /// If the estimated size reaches <see cref="AutoDisposeThreshold"/>, this method will block until this
        /// <see cref="GraphicsDevice"/> becomes idle,
        /// and then it will dispose all objects added through <see cref="AutoDisposeWhenIdle(IDisposable, long)"/>.
        /// <br/>
        /// To maintain backward compatibility, <see cref="IDisposable"/> objects added through <see cref="DisposeWhenIdle(IDisposable)"/>
        /// will <i>not</i> be automatically disposed by this method.
        /// They still need to be disposed by using <see cref="WaitForIdle()"/> or <see cref="Dispose()"/>.
        /// </summary>
        /// <param name="hintSize">The amount to be added to the current estimated size.</param>
        public void AddAutoDisposePressure(long hintSize)
        {
            AutoDisposeWhenIdle(null, hintSize);
        }

        private void FlushDeferredDisposals()
        {
            lock (_deferredDisposalLock)
            {
                foreach (IDisposable disposable in _disposables)
                {
                    disposable.Dispose();
                }
                _disposables.Clear();
                FlushDeferredAutoDisposals();
            }
        }

        private void FlushDeferredAutoDisposals()
        {
            lock (_deferredDisposalLock)
            {
                foreach (IDisposable disposable in _autoDisposables)
                {
                    disposable.Dispose();
                }
                _autoDisposables.Clear();
                _autoDisposeCounter = 0;
            }
        }

        /// <summary>
        /// Performs API-specific disposal of resources controlled by this instance.
        /// </summary>
        protected abstract void PlatformDispose();

        /// <summary>
        /// Creates and caches common device resources after device creation completes.
        /// </summary>
        protected void PostDeviceCreated()
        {
            PointSampler = ResourceFactory.CreateSampler(SamplerDescription.Point);
            LinearSampler = ResourceFactory.CreateSampler(SamplerDescription.Linear);
            if (Features.SamplerAnisotropy)
            {
                _aniso4xSampler = ResourceFactory.CreateSampler(SamplerDescription.Aniso4x);
            }
        }

        /// <summary>
        /// Gets a simple point-filtered <see cref="Sampler"/> object owned by this instance.
        /// This object is created with <see cref="SamplerDescription.Point"/>.
        /// </summary>
        public Sampler PointSampler { get; private set; }

        /// <summary>
        /// Gets a simple linear-filtered <see cref="Sampler"/> object owned by this instance.
        /// This object is created with <see cref="SamplerDescription.Linear"/>.
        /// </summary>
        public Sampler LinearSampler { get; private set; }

        /// <summary>
        /// Gets a simple 4x anisotropic-filtered <see cref="Sampler"/> object owned by this instance.
        /// This object is created with <see cref="SamplerDescription.Aniso4x"/>.
        /// This property can only be used when <see cref="GraphicsDeviceFeatures.SamplerAnisotropy"/> is supported.
        /// </summary>
        public Sampler Aniso4xSampler
        {
            get
            {
                if (!Features.SamplerAnisotropy)
                {
                    throw new VeldridException(
                        "GraphicsDevice.Aniso4xSampler cannot be used unless GraphicsDeviceFeatures.SamplerAnisotropy is supported.");
                }

                Debug.Assert(_aniso4xSampler != null);
                return _aniso4xSampler;
            }
        }

        /// <summary>
        /// Frees unmanaged resources controlled by this device.
        /// All created child resources must be Disposed prior to calling this method.
        /// </summary>
        public void Dispose()
        {
            WaitForIdle();
            PointSampler.Dispose();
            LinearSampler.Dispose();
            Aniso4xSampler.Dispose();
            PlatformDispose();
        }

#if !EXCLUDE_D3D11_BACKEND
        /// <summary>
        /// Tries to get a <see cref="BackendInfoD3D11"/> for this instance. This method will only succeed if this is a D3D11
        /// GraphicsDevice.
        /// </summary>
        /// <param name="info">If successful, this will contain the <see cref="BackendInfoD3D11"/> for this instance.</param>
        /// <returns>True if this is a D3D11 GraphicsDevice and the operation was successful. False otherwise.</returns>
        public virtual bool GetD3D11Info(out BackendInfoD3D11 info) { info = null; return false; }

        /// <summary>
        /// Gets a <see cref="BackendInfoD3D11"/> for this instance. This method will only succeed if this is a D3D11
        /// GraphicsDevice. Otherwise, this method will throw an exception.
        /// </summary>
        /// <returns>The <see cref="BackendInfoD3D11"/> for this instance.</returns>
        public BackendInfoD3D11 GetD3D11Info()
        {
            if (!GetD3D11Info(out BackendInfoD3D11 info))
            {
                throw new VeldridException($"{nameof(GetD3D11Info)} can only be used on a D3D11 GraphicsDevice.");
            }

            return info;
        }
#endif

#if !EXCLUDE_VULKAN_BACKEND
        /// <summary>
        /// Tries to get a <see cref="BackendInfoVulkan"/> for this instance. This method will only succeed if this is a Vulkan
        /// GraphicsDevice.
        /// </summary>
        /// <param name="info">If successful, this will contain the <see cref="BackendInfoVulkan"/> for this instance.</param>
        /// <returns>True if this is a Vulkan GraphicsDevice and the operation was successful. False otherwise.</returns>
        public virtual bool GetVulkanInfo(out BackendInfoVulkan info) { info = null; return false; }

        /// <summary>
        /// Gets a <see cref="BackendInfoVulkan"/> for this instance. This method will only succeed if this is a Vulkan
        /// GraphicsDevice. Otherwise, this method will throw an exception.
        /// </summary>
        /// <returns>The <see cref="BackendInfoVulkan"/> for this instance.</returns>
        public BackendInfoVulkan GetVulkanInfo()
        {
            if (!GetVulkanInfo(out BackendInfoVulkan info))
            {
                throw new VeldridException($"{nameof(GetVulkanInfo)} can only be used on a Vulkan GraphicsDevice.");
            }

            return info;
        }
#endif

#if !EXCLUDE_OPENGL_BACKEND
        /// <summary>
        /// Tries to get a <see cref="BackendInfoOpenGL"/> for this instance. This method will only succeed if this is an OpenGL
        /// GraphicsDevice.
        /// </summary>
        /// <param name="info">If successful, this will contain the <see cref="BackendInfoOpenGL"/> for this instance.</param>
        /// <returns>True if this is an OpenGL GraphicsDevice and the operation was successful. False otherwise.</returns>
        public virtual bool GetOpenGLInfo(out BackendInfoOpenGL info) { info = null; return false; }

        /// <summary>
        /// Gets a <see cref="BackendInfoOpenGL"/> for this instance. This method will only succeed if this is an OpenGL
        /// GraphicsDevice. Otherwise, this method will throw an exception.
        /// </summary>
        /// <returns>The <see cref="BackendInfoOpenGL"/> for this instance.</returns>
        public BackendInfoOpenGL GetOpenGLInfo()
        {
            if (!GetOpenGLInfo(out BackendInfoOpenGL info))
            {
                throw new VeldridException($"{nameof(GetOpenGLInfo)} can only be used on an OpenGL GraphicsDevice.");
            }

            return info;
        }
#endif

        /// <summary>
        /// Checks whether the given <see cref="GraphicsBackend"/> is supported on this system.
        /// </summary>
        /// <param name="backend">The GraphicsBackend to check.</param>
        /// <returns>True if the GraphicsBackend is supported; false otherwise.</returns>
        public static bool IsBackendSupported(GraphicsBackend backend)
        {
            switch (backend)
            {
                case GraphicsBackend.Direct3D11:
#if !EXCLUDE_D3D11_BACKEND
                    return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
                    return false;
#endif
                case GraphicsBackend.Vulkan:
#if !EXCLUDE_VULKAN_BACKEND
                    return Vk.VkGraphicsDevice.IsSupported();
#else
                    return false;
#endif
                case GraphicsBackend.OpenGL:
#if !EXCLUDE_OPENGL_BACKEND
                    return true;
#else
                    return false;
#endif
                case GraphicsBackend.Metal:
#if !EXCLUDE_METAL_BACKEND
                    return MTL.MTLGraphicsDevice.IsSupported();
#else
                    return false;
#endif
                case GraphicsBackend.OpenGLES:
#if !EXCLUDE_OPENGL_BACKEND
                    return !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#else
                    return false;
#endif
                default:
                    throw Illegal.Value<GraphicsBackend>();
            }
        }

#if !EXCLUDE_D3D11_BACKEND
        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Direct3D 11.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Direct3D 11 API.</returns>
        public static GraphicsDevice CreateD3D11(GraphicsDeviceOptions options)
        {
            return new D3D11.D3D11GraphicsDevice(options, new D3D11DeviceOptions(), null);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Direct3D 11, with a main Swapchain.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <param name="swapchainDescription">A description of the main Swapchain to create.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Direct3D 11 API.</returns>
        public static GraphicsDevice CreateD3D11(GraphicsDeviceOptions options, SwapchainDescription swapchainDescription)
        {
            return new D3D11.D3D11GraphicsDevice(options, new D3D11DeviceOptions(), swapchainDescription);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Direct3D 11.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <param name="d3d11Options">The Direct3D11-specific options used to create the device.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Direct3D 11 API.</returns>
        public static GraphicsDevice CreateD3D11(GraphicsDeviceOptions options, D3D11DeviceOptions d3d11Options)
        {
            return new D3D11.D3D11GraphicsDevice(options, d3d11Options, null);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Direct3D 11, with a main Swapchain.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <param name="d3d11Options">The Direct3D11-specific options used to create the device.</param>
        /// <param name="swapchainDescription">A description of the main Swapchain to create.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Direct3D 11 API.</returns>
        public static GraphicsDevice CreateD3D11(GraphicsDeviceOptions options, D3D11DeviceOptions d3d11Options, SwapchainDescription swapchainDescription)
        {
            return new D3D11.D3D11GraphicsDevice(options, d3d11Options, swapchainDescription);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Direct3D 11, with a main Swapchain.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <param name="hwnd">The Win32 window handle to render into.</param>
        /// <param name="width">The initial width of the window.</param>
        /// <param name="height">The initial height of the window.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Direct3D 11 API.</returns>
        public static GraphicsDevice CreateD3D11(GraphicsDeviceOptions options, IntPtr hwnd, uint width, uint height)
        {
            SwapchainDescription swapchainDescription = new SwapchainDescription(
                SwapchainSource.CreateWin32(hwnd, IntPtr.Zero),
                width, height,
                options.SwapchainDepthFormat,
                options.SyncToVerticalBlank,
                options.SwapchainSrgbFormat);

            return new D3D11.D3D11GraphicsDevice(options, new D3D11DeviceOptions(), swapchainDescription);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Direct3D 11, with a main Swapchain.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <param name="swapChainPanel">A COM object which must implement the <see cref="Vortice.DXGI.ISwapChainPanelNative"/>
        /// or <see cref="Vortice.DXGI.ISwapChainBackgroundPanelNative"/> interface. Generally, this should be a SwapChainPanel
        /// or SwapChainBackgroundPanel contained in your application window.</param>
        /// <param name="renderWidth">The renderable width of the swapchain panel.</param>
        /// <param name="renderHeight">The renderable height of the swapchain panel.</param>
        /// <param name="logicalDpi">The logical DPI of the swapchain panel.</param>
        /// <returns></returns>
        public static GraphicsDevice CreateD3D11(
            GraphicsDeviceOptions options,
            object swapChainPanel,
            double renderWidth,
            double renderHeight,
            float logicalDpi)
        {
            SwapchainDescription swapchainDescription = new SwapchainDescription(
                SwapchainSource.CreateUwp(swapChainPanel, logicalDpi),
                (uint)renderWidth,
                (uint)renderHeight,
                options.SwapchainDepthFormat,
                options.SyncToVerticalBlank,
                options.SwapchainSrgbFormat);

            return new D3D11.D3D11GraphicsDevice(options, new D3D11DeviceOptions(), swapchainDescription);
        }
#endif

#if !EXCLUDE_VULKAN_BACKEND
        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Vulkan.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Vulkan API.</returns>
        public static GraphicsDevice CreateVulkan(GraphicsDeviceOptions options)
        {
            return new Vk.VkGraphicsDevice(options, null);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Vulkan.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <param name="vkOptions">The Vulkan-specific options used to create the device.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Vulkan API.</returns>
        public static GraphicsDevice CreateVulkan(GraphicsDeviceOptions options, VulkanDeviceOptions vkOptions)
        {
            return new Vk.VkGraphicsDevice(options, null, vkOptions);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Vulkan, with a main Swapchain.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <param name="swapchainDescription">A description of the main Swapchain to create.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Vulkan API.</returns>
        public static GraphicsDevice CreateVulkan(GraphicsDeviceOptions options, SwapchainDescription swapchainDescription)
        {
            return new Vk.VkGraphicsDevice(options, swapchainDescription);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Vulkan, with a main Swapchain.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <param name="vkOptions">The Vulkan-specific options used to create the device.</param>
        /// <param name="swapchainDescription">A description of the main Swapchain to create.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Vulkan API.</returns>
        public static GraphicsDevice CreateVulkan(
            GraphicsDeviceOptions options,
            SwapchainDescription swapchainDescription,
            VulkanDeviceOptions vkOptions)
        {
            return new Vk.VkGraphicsDevice(options, swapchainDescription, vkOptions);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Vulkan, with a main Swapchain.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <param name="surfaceSource">The source from which a Vulkan surface can be created.</param>
        /// <param name="width">The initial width of the window.</param>
        /// <param name="height">The initial height of the window.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Vulkan API.</returns>
        public static GraphicsDevice CreateVulkan(GraphicsDeviceOptions options, Vk.VkSurfaceSource surfaceSource, uint width, uint height)
        {
            SwapchainDescription scDesc = new SwapchainDescription(
                surfaceSource.GetSurfaceSource(),
                width, height,
                options.SwapchainDepthFormat,
                options.SyncToVerticalBlank,
                options.SwapchainSrgbFormat);

            return new Vk.VkGraphicsDevice(options, scDesc);
        }
#endif

#if !EXCLUDE_OPENGL_BACKEND
        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using OpenGL or OpenGL ES, with a main Swapchain.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <param name="platformInfo">An <see cref="OpenGL.OpenGLPlatformInfo"/> object encapsulating necessary OpenGL context
        /// information.</param>
        /// <param name="width">The initial width of the window.</param>
        /// <param name="height">The initial height of the window.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the OpenGL or OpenGL ES API.</returns>
        public static GraphicsDevice CreateOpenGL(
            GraphicsDeviceOptions options,
            OpenGL.OpenGLPlatformInfo platformInfo,
            uint width,
            uint height)
        {
            return new OpenGL.OpenGLGraphicsDevice(options, platformInfo, width, height);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using OpenGL ES, with a main Swapchain.
        /// This overload can only be used on iOS or Android to create a GraphicsDevice for an Android Surface or an iOS UIView.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <param name="swapchainDescription">A description of the main Swapchain to create.
        /// The SwapchainSource must have been created from an Android Surface or an iOS UIView.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the OpenGL or OpenGL ES API.</returns>
        public static GraphicsDevice CreateOpenGLES(
            GraphicsDeviceOptions options,
            SwapchainDescription swapchainDescription)
        {
            return new OpenGL.OpenGLGraphicsDevice(options, swapchainDescription);
        }
#endif

#if !EXCLUDE_METAL_BACKEND
        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Metal.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Metal API.</returns>
        public static GraphicsDevice CreateMetal(GraphicsDeviceOptions options)
        {
            return new MTL.MTLGraphicsDevice(options, null);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Metal, with a main Swapchain.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <param name="swapchainDescription">A description of the main Swapchain to create.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Metal API.</returns>
        public static GraphicsDevice CreateMetal(GraphicsDeviceOptions options, SwapchainDescription swapchainDescription)
        {
            return new MTL.MTLGraphicsDevice(options, swapchainDescription);
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Metal, with a main Swapchain.
        /// </summary>
        /// <param name="options">Describes several common properties of the GraphicsDevice.</param>
        /// <param name="nsWindow">A pointer to an NSWindow object, which will be used to create the Metal device's swapchain.
        /// </param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Metal API.</returns>
        public static GraphicsDevice CreateMetal(GraphicsDeviceOptions options, IntPtr nsWindow)
        {
            SwapchainDescription swapchainDesc = new SwapchainDescription(
                new NSWindowSwapchainSource(nsWindow),
                0, 0,
                options.SwapchainDepthFormat,
                options.SyncToVerticalBlank,
                options.SwapchainSrgbFormat);

            return new MTL.MTLGraphicsDevice(options, swapchainDesc);
        }
#endif
    }
}
