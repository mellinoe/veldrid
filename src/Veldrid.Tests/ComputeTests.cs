using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid.SPIRV;
using Xunit;

namespace Veldrid.Tests
{
    [StructLayout(LayoutKind.Sequential)]
    struct FillValueStruct
    {
        /// <summary>
        /// The value we fill the 3d texture with.
        /// </summary>
        public float FillValue;
        public float pad1, pad2, pad3;

        public FillValueStruct(float fillValue)
        {
            FillValue = fillValue;
            pad1 = pad2 = pad3 = 0;
        }
    }


    public abstract class ComputeTests<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void ComputeShader3dTexture()
        {
            // Just a dumb compute shader that fills a 3D texture with the same value from a uniform multiplied by the depth.
            string shaderText = @"
#version 450
layout(set = 0, binding = 0, rgba32f) uniform image3D TextureToFill;
layout(set = 0, binding = 1) uniform FillValueBuffer
{
    float FillValue;
    float Padding1;
    float Padding2;
    float Padding3;
};
layout(local_size_x = 16, local_size_y = 16, local_size_z = 1) in;
void main()
{
    ivec3 textureCoordinate = ivec3(gl_GlobalInvocationID.xyz);
    float dataToStore = FillValue * (textureCoordinate.z + 1);

    imageStore(TextureToFill, textureCoordinate, vec4(dataToStore));
}
";

            const float FillValue = 42.42f;
            const uint OutputTextureSize = 32;

            using Shader computeShader = RF.CreateFromSpirv(new ShaderDescription(
                ShaderStages.Compute,
                Encoding.ASCII.GetBytes(shaderText),
                "main"));

            using ResourceLayout computeLayout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("TextureToFill", ResourceKind.TextureReadWrite, ShaderStages.Compute),
                new ResourceLayoutElementDescription("FillValueBuffer", ResourceKind.UniformBuffer, ShaderStages.Compute)));

            using Pipeline computePipeline = RF.CreateComputePipeline(new ComputePipelineDescription(
                computeShader,
                computeLayout,
                16, 16, 1));

            using DeviceBuffer fillValueBuffer = RF.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<FillValueStruct>(), BufferUsage.UniformBuffer));

            // Create our output texture.
            using Texture computeTargetTexture = RF.CreateTexture(TextureDescription.Texture3D(
                OutputTextureSize,
                OutputTextureSize,
                OutputTextureSize,
                1,
                PixelFormat.R32_G32_B32_A32_Float,
                TextureUsage.Sampled | TextureUsage.Storage));

            using TextureView computeTargetTextureView = RF.CreateTextureView(computeTargetTexture);

            using ResourceSet computeResourceSet = RF.CreateResourceSet(new ResourceSetDescription(
                computeLayout,
                computeTargetTextureView,
                fillValueBuffer));

            using CommandList cl = RF.CreateCommandList();
            cl.Begin();

            cl.UpdateBuffer(fillValueBuffer, 0, new FillValueStruct(FillValue));

            // Use the compute shader to fill the texture.
            cl.SetPipeline(computePipeline);
            cl.SetComputeResourceSet(0, computeResourceSet);
            const uint GroupDivisorXY = 16;
            cl.Dispatch(OutputTextureSize / GroupDivisorXY, OutputTextureSize / GroupDivisorXY, OutputTextureSize);

            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            // Read back from our texture and make sure it has been properly filled.
            for (uint depth = 0; depth < computeTargetTexture.Depth; depth++)
            {
                RgbaFloat expectedFillValue = new RgbaFloat(new System.Numerics.Vector4(FillValue * (depth + 1)));
                int notFilledCount = CountTexelsNotFilledAtDepth(GD, computeTargetTexture, expectedFillValue, depth);

                if (notFilledCount == 0)
                {
                    // Expected behavior:
                    Console.WriteLine($"All texels were properly set at depth {depth}");
                }
                else
                {
                    // Unexpected behavior:
                    uint totalTexels = computeTargetTexture.Width * computeTargetTexture.Height;
                    throw new Exception($"{notFilledCount} of {totalTexels} texels were not properly set at depth {depth}");
                }
            }
        }


        /// <summary>
        /// Returns the number of texels in the texture that DO NOT match the fill value.
        /// </summary>
        private static int CountTexelsNotFilledAtDepth<TexelType>(GraphicsDevice device, Texture texture, TexelType fillValue, uint depth)
            where TexelType : unmanaged
        {
            ResourceFactory factory = device.ResourceFactory;

            // We need to create a staging texture and copy into it.
            TextureDescription description = new TextureDescription(texture.Width, texture.Height, depth: 1,
                texture.MipLevels, texture.ArrayLayers,
                texture.Format, TextureUsage.Staging,
                texture.Type, texture.SampleCount);

            Texture staging = factory.CreateTexture(ref description);

            using CommandList cl = factory.CreateCommandList();
            cl.Begin();

            cl.CopyTexture(texture,
                srcX: 0, srcY: 0, srcZ: depth,
                srcMipLevel: 0, srcBaseArrayLayer: 0,
                staging,
                dstX: 0, dstY: 0, dstZ: 0,
                dstMipLevel: 0, dstBaseArrayLayer: 0,
                staging.Width, staging.Height,
                depth: 1, layerCount: 1);

            cl.End();
            device.SubmitCommands(cl);
            device.WaitForIdle();

            try
            {
                MappedResourceView<TexelType> mapped = device.Map<TexelType>(staging, MapMode.Read);

                int notFilledCount = 0;
                for (int y = 0; y < staging.Height; y++)
                {
                    for (int x = 0; x < staging.Width; x++)
                    {
                        TexelType actual = mapped[x, y];
                        if (!fillValue.Equals(actual))
                        {
                            notFilledCount++;
                        }
                    }
                }

                return notFilledCount;
            }
            finally
            {
                device.Unmap(staging);
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct BasicComputeTestParams
        {
            public uint Width;
            public uint Height;
            private uint _padding1;
            private uint _padding2;
        }


        [Fact]
        public void BasicCompute()
        {
            if (!GD.Features.ComputeShader)
            {
                return;
            }

            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Params", ResourceKind.UniformBuffer, ShaderStages.Compute),
                new ResourceLayoutElementDescription("Source", ResourceKind.StructuredBufferReadWrite, ShaderStages.Compute),
                new ResourceLayoutElementDescription("Destination", ResourceKind.StructuredBufferReadWrite, ShaderStages.Compute)));

            uint width = 1024;
            uint height = 1024;
            DeviceBuffer paramsBuffer = RF.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<BasicComputeTestParams>(), BufferUsage.UniformBuffer));
            DeviceBuffer sourceBuffer = RF.CreateBuffer(new BufferDescription(width * height * 4, BufferUsage.StructuredBufferReadWrite, 4));
            DeviceBuffer destinationBuffer = RF.CreateBuffer(new BufferDescription(width * height * 4, BufferUsage.StructuredBufferReadWrite, 4));

            GD.UpdateBuffer(paramsBuffer, 0, new BasicComputeTestParams { Width = width, Height = height });

            float[] sourceData = new float[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int index = y * (int)width + x;
                    sourceData[index] = index;
                }
            GD.UpdateBuffer(sourceBuffer, 0, sourceData);

            ResourceSet rs = RF.CreateResourceSet(new ResourceSetDescription(layout, paramsBuffer, sourceBuffer, destinationBuffer));

            Pipeline pipeline = RF.CreateComputePipeline(new ComputePipelineDescription(
                TestShaders.LoadCompute(RF, "BasicComputeTest"),
                layout,
                16, 16, 1));

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetPipeline(pipeline);
            cl.SetComputeResourceSet(0, rs);
            cl.Dispatch(width / 16, width / 16, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            DeviceBuffer sourceReadback = GetReadback(sourceBuffer);
            DeviceBuffer destinationReadback = GetReadback(destinationBuffer);

            MappedResourceView<float> sourceReadView = GD.Map<float>(sourceReadback, MapMode.Read);
            MappedResourceView<float> destinationReadView = GD.Map<float>(destinationReadback, MapMode.Read);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int index = y * (int)width + x;
                    Assert.Equal(2 * sourceData[index], sourceReadView[index]);
                    Assert.Equal(sourceData[index], destinationReadView[index]);
                }

            GD.Unmap(sourceReadback);
            GD.Unmap(destinationReadback);
        }

        [Theory]
        [MemberData(nameof(FillBuffer_WithOffsetsData))]
        public void FillBuffer_WithOffsets(uint srcSetMultiple, uint srcBindingMultiple, uint dstSetMultiple, uint dstBindingMultiple, bool combinedLayout)
        {
            if (!GD.Features.ComputeShader) { return; }
            Debug.Assert((GD.StructuredBufferMinOffsetAlignment % sizeof(uint)) == 0);

            uint valueCount = 512;
            uint dataSize = valueCount * sizeof(uint);
            uint totalSrcAlignment = GD.StructuredBufferMinOffsetAlignment * (srcSetMultiple + srcBindingMultiple);
            uint totalDstAlignment = GD.StructuredBufferMinOffsetAlignment * (dstSetMultiple + dstBindingMultiple);

            DeviceBuffer copySrc = RF.CreateBuffer(
                new BufferDescription(totalSrcAlignment + dataSize, BufferUsage.StructuredBufferReadOnly, sizeof(uint)));
            DeviceBuffer copyDst = RF.CreateBuffer(
                new BufferDescription(totalDstAlignment + dataSize, BufferUsage.StructuredBufferReadWrite, sizeof(uint)));

            ResourceLayout[] layouts;
            ResourceSet[] sets;

            DeviceBufferRange srcRange = new DeviceBufferRange(copySrc, srcSetMultiple * GD.StructuredBufferMinOffsetAlignment, dataSize);
            DeviceBufferRange dstRange = new DeviceBufferRange(copyDst, dstSetMultiple * GD.StructuredBufferMinOffsetAlignment, dataSize);

            if (combinedLayout)
            {
                layouts = new[]
                {
                    RF.CreateResourceLayout(new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription(
                            "CopySrc",
                            ResourceKind.StructuredBufferReadOnly,
                            ShaderStages.Compute,
                            ResourceLayoutElementOptions.DynamicBinding),
                        new ResourceLayoutElementDescription(
                            "CopyDst",
                            ResourceKind.StructuredBufferReadWrite,
                            ShaderStages.Compute,
                            ResourceLayoutElementOptions.DynamicBinding)))
                };
                sets = new[]
                {
                    RF.CreateResourceSet(new ResourceSetDescription(layouts[0], srcRange, dstRange))
                };
            }
            else
            {
                layouts = new[]
                {
                    RF.CreateResourceLayout(new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription(
                            "CopySrc",
                            ResourceKind.StructuredBufferReadOnly,
                            ShaderStages.Compute,
                            ResourceLayoutElementOptions.DynamicBinding))),
                    RF.CreateResourceLayout(new ResourceLayoutDescription(
                        new ResourceLayoutElementDescription(
                            "CopyDst",
                            ResourceKind.StructuredBufferReadWrite,
                            ShaderStages.Compute,
                            ResourceLayoutElementOptions.DynamicBinding)))
                };
                sets = new[]
                {
                    RF.CreateResourceSet(new ResourceSetDescription(layouts[0], srcRange)),
                    RF.CreateResourceSet(new ResourceSetDescription(layouts[1], dstRange)),
                };
            }

            Pipeline pipeline = RF.CreateComputePipeline(new ComputePipelineDescription(
                TestShaders.LoadCompute(RF, combinedLayout ? "FillBuffer" : "FillBuffer_SeparateLayout"),
                layouts,
                1, 1, 1));

            uint[] srcData = Enumerable.Range(0, (int)copySrc.SizeInBytes / sizeof(uint)).Select(i => (uint)i).ToArray();
            GD.UpdateBuffer(copySrc, 0, srcData);

            CommandList cl = RF.CreateCommandList();
            cl.Begin();
            cl.SetPipeline(pipeline);
            if (combinedLayout)
            {
                uint[] offsets = new[]
                {
                    srcBindingMultiple * GD.StructuredBufferMinOffsetAlignment,
                    dstBindingMultiple * GD.StructuredBufferMinOffsetAlignment
                };
                cl.SetComputeResourceSet(0, sets[0], offsets);
            }
            else
            {
                uint offset = srcBindingMultiple * GD.StructuredBufferMinOffsetAlignment;
                cl.SetComputeResourceSet(0, sets[0], 1, ref offset);
                offset = dstBindingMultiple * GD.StructuredBufferMinOffsetAlignment;
                cl.SetComputeResourceSet(1, sets[1], 1, ref offset);
            }
            cl.Dispatch(512, 1, 1);
            cl.End();
            GD.SubmitCommands(cl);
            GD.WaitForIdle();

            DeviceBuffer readback = GetReadback(copyDst);

            MappedResourceView<uint> readView = GD.Map<uint>(readback, MapMode.Read);
            for (uint i = 0; i < valueCount; i++)
            {
                uint srcIndex = totalSrcAlignment / sizeof(uint) + i;
                uint expected = srcData[(int)srcIndex];

                uint dstIndex = totalDstAlignment / sizeof(uint) + i;
                uint actual = readView[dstIndex];

                Assert.Equal(expected, actual);
            }
            GD.Unmap(readback);
        }

        public static IEnumerable<object[]> FillBuffer_WithOffsetsData()
        {
            foreach (uint srcSetMultiple in new[] { 0, 2, 10 })
                foreach (uint srcBindingMultiple in new[] { 0, 2, 10 })
                    foreach (uint dstSetMultiple in new[] { 0, 2, 10 })
                        foreach (uint dstBindingMultiple in new[] { 0, 2, 10 })
                            foreach (bool combinedLayout in new[] { false, true })
                            {
                                yield return new object[] { srcSetMultiple, srcBindingMultiple, dstSetMultiple, dstBindingMultiple, combinedLayout };
                            }
        }
    }

#if TEST_OPENGL
    [Trait("Backend", "OpenGL")]
    public class OpenGLComputeTests : ComputeTests<OpenGLDeviceCreator> { }
#endif
#if TEST_OPENGLES
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESComputeTests : ComputeTests<OpenGLESDeviceCreator> { }
#endif
#if TEST_VULKAN
    [Trait("Backend", "Vulkan")]
    public class VulkanComputeTests : ComputeTests<VulkanDeviceCreatorWithMainSwapchain> { }
#endif
#if TEST_D3D11
    [Trait("Backend", "D3D11")]
    public class D3D11ComputeTests : ComputeTests<D3D11DeviceCreatorWithMainSwapchain> { }
#endif
#if TEST_METAL
    [Trait("Backend", "Metal")]
    public class MetalComputeTests : RenderTests<MetalDeviceCreator> { }
#endif
}
